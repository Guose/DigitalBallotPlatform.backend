﻿namespace DigitalBallotPlatform.Watermark.DTOs
{
    public class WatermarkDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public WatermarkDTO(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}