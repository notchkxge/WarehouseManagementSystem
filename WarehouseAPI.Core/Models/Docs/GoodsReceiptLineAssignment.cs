using System.ComponentModel.DataAnnotations.Schema;
using WarehouseAPI.Core.Models.Docs;

namespace WarehouseAPI.Core.Models.Entities
{
    [Table("GoodsReceiptLineAssignments")]
    public class GoodsReceiptLineAssignment
    {
        public int Id { get; set; }
        public int DocumentLineId { get; set; }
        public int StorageLocationId { get; set; }
        
        // Navigation properties
        public DocumentLine DocumentLine { get; set; }
        public StorageLocation StorageLocation { get; set; }
    }
}