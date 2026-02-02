using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using SutureHealth.DataScraping;
using System.Text.Json;
using static Amazon.Lambda.SQSEvents.SQSEvent;

namespace SutureHealth.DataScraping.Services.Lambda
{
    public partial class Function
    {
        public async Task InsertUpdatePatient(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                MessageAttribute messageAttributeValue;
                message.MessageAttributes.TryGetValue("RequestType", out messageAttributeValue);

                if (messageAttributeValue.StringValue == "SimplePatient")
                {
                    context.Logger.LogInformation($"Simple Patient Request Type");
                    await ProcessMessageForPatientAsync(message, context);
                }
                else
                {
                    context.Logger.LogInformation($"Detailed Patient Request Type");
                    await ProcessMessageForPatientDetailAsync(message, context);
                }
            }
        }

        private async Task ProcessMessageForPatientDetailAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            try
            {
                var patientDetail = JsonSerializer.Deserialize<ScrapedPatientDetail>(message.Body);
                var dbpatientDetail = await DataScrapingService.GetScrapedPatientDetailById(patientDetail.ExternalId);

                if (dbpatientDetail != default && dbpatientDetail != null)
                {
                    context.Logger.LogInformation($"Update Patient with Id = {dbpatientDetail.Id}");

                    if (DateTime.Compare(dbpatientDetail.CreatedAt, patientDetail.CreatedAt) >= 0)
                    {
                        context.Logger.LogInformation($"Incoming data is older");
                        return;
                    }

                    dbpatientDetail = dbpatientDetail.Update(patientDetail.FirstName, patientDetail.MiddleName,
                                           patientDetail.LastName, patientDetail.DateOfBirth,
                                           patientDetail.Gender, patientDetail.MaritalStatus,
                                           patientDetail.SexualOrientation, patientDetail.AttendedPhysician,
                                           patientDetail.Address, patientDetail.City,
                                           patientDetail.Country, patientDetail.PostalCode,
                                           patientDetail.PatientBalance, patientDetail.InsuranceBalance,
                                           patientDetail.TotalBalance, patientDetail.Occupation,
                                           patientDetail.Employer, patientDetail.Language,
                                           patientDetail.Ethnicity, patientDetail.Race,
                                           patientDetail.FamilySize, patientDetail.MonthlyIncome,
                                           patientDetail.Religion, patientDetail.DeceaseDate,
                                           patientDetail.DeceaseReason, patientDetail.SSN,
                                           patientDetail.CreatedAt,
                                           patientDetail.Observations,
                                           patientDetail.Contacts,
                                           patientDetail.Conditions,
                                           patientDetail.Allergies,
                                           patientDetail.Medications,
                                           patientDetail.Immunizations,
                                           patientDetail.Prescriptions,
                                           patientDetail.Procedures);

                    await DataScrapingService.UpdateScrapedPatientDetail(dbpatientDetail);
                }
                else
                {
                    context.Logger.LogInformation($"Create Patient with Id = {patientDetail.Id}");
                    await DataScrapingService.CreateScrapedPatientDetail(patientDetail);
                }
            }
            catch (Exception e)
            {
                context.Logger.LogInformation($"detail sql error log {e.Message} /// {e.InnerException}");

            }

            await Task.CompletedTask;
        }

        private async Task ProcessMessageForPatientAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            try
            {
                context.Logger.LogInformation($"ProcessMessageForPatientAsync function");
                context.Logger.LogInformation($"ProcessMessageForPatientAsync patient : {message.Body}" );

                var patient = JsonSerializer.Deserialize<ScrapedPatient>(message.Body);
                var dbPatient = await DataScrapingService.GetScrapedPatientById(patient.ExternalId);

                if (dbPatient != default && dbPatient != null)
                {
                    if (DateTime.Compare(dbPatient.CreatedAt, patient.CreatedAt) >= 0)
                    {
                        context.Logger.LogInformation($"Incoming data is older");
                        return;
                    }
                    context.Logger.LogInformation($"Update Patient");
                    dbPatient = dbPatient.Update(patient.FirstName, patient.MiddleName, patient.LastName, patient.Phone, patient.SSN, patient.DateOfBirth, patient.AttendedPhysician, patient.CreatedAt);

                    await DataScrapingService.UpdateScrapedPatient(dbPatient);
                }
                else
                {
                    context.Logger.LogInformation($"Create Patient");
                    await DataScrapingService.CreateScrapedPatient(patient);
                }
            }
            catch (Exception e)
            {
                context.Logger.LogInformation($"non detail DataScrapingService.CreateScrapedPatient error {e.Message} /// {e.InnerException}");

            }

            await Task.CompletedTask;
        }
    }   
}
