using Microsoft.EntityFrameworkCore;
using Moq;
using SutureHealth.Patients;
using SutureHealth.Patients.Services;
using SutureHealth.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace SutureHealth.AspNetCore.WebHost.Testing.Area.Admin.Controllers
{
    internal class PatientServicesProviderMock: Mock<IPatientServicesProvider>
    {
        protected PatientDbContext DbContext { get; private set; }

        public PatientServicesProviderMock(PatientDbContext patientContext)
        {
            DbContext = patientContext;
        }

        public PatientServicesProviderMock() 
        {
            _ = Setup(x => x.GetMatchLogByFilter(It.IsAny<Expression<Func<MatchLog, bool>>>())).Returns(GetMatchLogByFilter);
            _ = Setup(x => x.GetMatchLogByIdAsync(It.IsAny<int>())).Returns(GetMatchLogByIdAsync);
            _ = Setup(x => x.UpdateAsync(It.IsAny<Patients.Patient>(), It.IsAny<int>(), It.IsAny<int>())).Returns(UpdateAsync);
            _ = Setup(x => x.SetPatientMatchLogFlagsToResolved(It.IsAny<int>(), It.IsAny<int>())).Returns(SetPatientMatchLogFlagsToResolved);
            _ = Setup(x => x.TryDisableNeedReviewForMatchLogInstance(It.IsAny<int>())).Returns(TryDisableNeedReviewForMatchLogInstance);
            _ = Setup(x => x.CreateAsync(It.IsAny<Patients.Patient>(), It.IsAny<int>(), It.IsAny<int>())).Returns(CreateAsync);
        }

        IQueryable<MatchLog> GetMatchLogByFilter(Expression<Func<MatchLog, bool>> predicate) 
                            => DbContext.GetMatchLogByFilter(predicate);

        Task<MatchLog> GetMatchLogByIdAsync(int matchPatientLogId) 
                            => DbContext.GetMatchLogByIdAsync(matchPatientLogId);


        async Task UpdateAsync(Patients.Patient patient, int organizationId, int memberId)
                            => await DbContext.UpdatePatientAsync(patient, organizationId, memberId);

        async Task SetPatientMatchLogFlagsToResolved(int matchLogId, int userId) 
        {
            var logItem = await DbContext.MatchLogs.FirstOrDefaultAsync(i => i.MatchPatientLogID == matchLogId);
            logItem.ManuallyMatched = true;
            logItem.ManuallyMatchedOn = DateTime.UtcNow;
            logItem.ManuallyMatchedBy = userId;
            await DbContext.SaveChangesAsync();

        }

        Task<bool> TryDisableNeedReviewForMatchLogInstance(int matchlogId) 
        {
            throw new NotImplementedException();
        }

        Task<Patients.Patient> CreateAsync(Patients.Patient patient, int organizationId, int memberId) 
        {
            throw new NotImplementedException();
        }

    }
}
