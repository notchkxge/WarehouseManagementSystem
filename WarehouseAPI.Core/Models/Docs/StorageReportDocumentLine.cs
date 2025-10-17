using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseAPI.Core.Models.Entities
{
    [Table("StorageReportDocumentLines")]
    public class StorageReportDocumentLine
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int StorageLocationId { get; set; }
        public double CurrentWeight { get; set; }
        public decimal VolumePercentage { get; set; }
        public DateTime RecordedDate { get; set; }

        // Navigation properties
        public StorageReportDocument Document { get; set; }
        public StorageLocation StorageLocation { get; set; }
    }
}