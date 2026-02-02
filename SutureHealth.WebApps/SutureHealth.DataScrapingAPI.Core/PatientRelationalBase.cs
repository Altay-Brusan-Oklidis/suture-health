using System.ComponentModel.DataAnnotations.Schema;

namespace SutureHealth.DataScraping
{
    public abstract class PatientRelationalBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
    }
}
