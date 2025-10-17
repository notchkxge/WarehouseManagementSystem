using System.ComponentModel.DataAnnotations.Schema;
using WarehouseAPI.Core.Models.Docs;

namespace WarehouseAPI.Core.Models.Entities
{
    public class StorageReportDocument : Document
    {
        public ICollection<StorageReportDocumentLine> DocumentLines { get; set; } = new List<StorageReportDocumentLine>();
    }
}