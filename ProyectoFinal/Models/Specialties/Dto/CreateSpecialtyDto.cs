﻿namespace ProyectoFinal.Models.Specialties.Dto
{
    public class CreateSpecialtyDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
