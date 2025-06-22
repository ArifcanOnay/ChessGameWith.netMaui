namespace SatrancApi.Entities.Models
{
    public abstract class BaseEntitiy
    {
        // Oluşturma bilgileri
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }  // Email adresi

        // Güncelleme bilgileri  
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }  // Email adresi

        // Soft delete bilgileri
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }  // Email adresi
    }
}
