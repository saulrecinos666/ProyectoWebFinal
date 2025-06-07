namespace ProyectoFinal.Models.Permissions.Dto
{
    public class ResponsePermissionDto
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}