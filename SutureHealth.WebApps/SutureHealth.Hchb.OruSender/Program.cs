// See https://aka.ms/new-console-template for more information


using System.Data.SqlTypes;
using System.Globalization;
using System.Text.Json;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SutureHealth.DataStream;
using SutureHealth.Hchb.OruSender;
using SutureHealth.Hchb.Services;
using SutureHealth.Requests;


int? hchbSubmitterId = 3045911;

var applicationHost = new ApplicationHostFactory();

var hchbWebDbContext = applicationHost.Services.GetRequiredService<HchbWebDbContext>();

var logger = applicationHost.Services.GetRequiredService<ILogger<Program>>();
var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
logger.LogInformation("{Now}", now);


var minDate = DateTime.UtcNow.AddDays(-2d);

var hchbRequestStatuses = hchbWebDbContext.RequestStatuses
    .Where(x => x.SubmitterId == hchbSubmitterId && 
                (x.Status == 1 || x.Status == 2) && 
                x.StDate >= minDate)
    .AsNoTracking();

Func<string, string> parseRejectionReason = data =>
{
    data = data
        .Replace("<RejectionSelections></RejectionSelections>", string.Empty)
        .Replace("Reason(s) for rejection: ", string.Empty);
    return data.Contains("|") ? data.Split("|")[0] : data;
};

var requestTasks = hchbRequestStatuses
    .Where(x => x.Status == 2)
    .Join(hchbWebDbContext.Tasks, x => x.St, x => x.TaskId,
        (request, task) => new
        {
            RequestId = request.Id,
            SubmitterId = request.SubmitterId,
            Status = request.Status,
            Gender = request.RequestPatient.Gender,
            PatientId = request.Patient,
            DateOfBirth = request.RequestPatient.DateOfBirth.ToString("yyyyMMdd"),
            PatientFirstName = request.RequestPatient.FirstName,
            PatientLastName = request.RequestPatient.LastName,
            SignedDate = request.StDate.ToString("yyyyMMddHHmmss"),
            RejectionReason = parseRejectionReason(task.Data),
            RejectionDate = task.CreateDate.ToString("yyyyMMddHHmmss"),
            RejectionUserId = task.SubmittedBy,
            ActionId = task.ActionId
        })
    .ToList();

var notRejections = requestTasks.Count(x => x.ActionId != 529);
if (notRejections > 0)
    logger.LogError("Found {Count} non-529 rejection tasks", notRejections);

var requestTransactions = hchbRequestStatuses
    .Join(hchbWebDbContext.HchbTransactions, x => x.Id, x => x.RequestId,
        (request, transaction) => new
        {
            RequestId = request.Id,
            SubmitterId = request.SubmitterId,
            Status = request.Status,
            TransactionId = transaction.Id,
            OrderNumber = transaction.OrderNumber,
            Gender = request.RequestPatient.Gender,
            PatientId = request.Patient,
            DateOfBirth = request.RequestPatient.DateOfBirth.ToString("yyyyMMdd"),
            PatientFirstName = request.RequestPatient.FirstName,
            PatientLastName = request.RequestPatient.LastName,
            SignedDate = request.StDate.ToString("yyyyMMddHHmmss")
        });


var signedRequestTransactions = requestTransactions
    .Where(x => x.Status == 1);

var rejectedRequestTransactions = requestTransactions
    .Where(x => x.Status == 2);

logger.LogInformation("Examining {Count} Signed", signedRequestTransactions.Count());
logger.LogInformation("Examining {Count} Rejected", rejectedRequestTransactions.Count());

var s3Service = applicationHost.Services.GetRequiredService<IAmazonS3>();

var i = 0;
foreach (var requestTransaction in requestTransactions)
{
    i += 1;
    try
    {
        var obj = await s3Service.GetObjectAsync("hl7message-production", 
            $"ORU/raw/{requestTransaction.RequestId}-{requestTransaction.OrderNumber}.txt");
        logger.LogDebug(
            "{i}: Found {OrderNumber} {RequestId} {Status}", 
            i, 
            requestTransaction.OrderNumber, 
            requestTransaction.RequestId, 
            requestTransaction.Status
        );
    }
    catch (AmazonS3Exception ex)
    {
        if (ex.Message != "The specified key does not exist.")
        {
            logger.LogError(ex.Message);
        }
        else
        {
            logger.LogInformation("Not Found {RequestId}-{OrderNumber}. Creating ORU", requestTransaction.RequestId,
                requestTransaction.OrderNumber);
            if (!string.IsNullOrWhiteSpace(ex.Message))
            {
                logger.LogError(ex.Message);
            }

        OruStream? oruStream = null;
        if (requestTransaction.Status == 1)
        {
            // If Signed
            oruStream = new OruStream
            {
                RequestId = requestTransaction.RequestId,
                Gender = requestTransaction.Gender,
                PatientId = requestTransaction.PatientId,
                BirthDate = requestTransaction.DateOfBirth,
                FirstName = requestTransaction.PatientFirstName,
                LastName = requestTransaction.PatientLastName,
                RejectReason = string.Empty,
                ResultDate = requestTransaction.SignedDate,
                ResultStatus = "F",
            };
        }
        else if (requestTransaction.Status == 2)
        {
            var rejectTask = requestTasks
                .Single(x => x.RequestId == requestTransaction.RequestId);
                
            // If Rejected
            oruStream = new OruStream
            {
                RequestId = requestTransaction.RequestId,
                Gender = requestTransaction.Gender,
                PatientId = requestTransaction.PatientId,
                BirthDate = requestTransaction.DateOfBirth,
                FirstName = requestTransaction.PatientFirstName,
                LastName = requestTransaction.PatientLastName,
                RejectReason = rejectTask.RejectionReason,
                ResultDate = rejectTask.RejectionDate,
                ResultStatus = "X",
            };
        }
        else
        {
            // Neither
            logger.LogError("Should not send ORU for Request with Status {Status}", 
                requestTransaction.Status);
        }

            if (oruStream != null)
            {
                logger.LogInformation("Sending ORU {Oru}", JsonSerializer.Serialize(oruStream));

                var response = await KinesisService.PutJsonToStreamAsync(
                    "HL7DataStream-Production", 
                    "ORU", 
                    JsonSerializer.Serialize<OruStream>(oruStream)
                );
                Console.WriteLine(response.HttpStatusCode);
                
                Thread.Sleep(5000);
            }
        }

    }

}