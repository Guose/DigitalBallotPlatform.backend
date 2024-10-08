﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalBallotPlatform.Shared.Models
{
    public class WatermarkColorModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Color { get; set; } = string.Empty;
        [Required]
        public string Tint { get; set; } = string.Empty;
        public bool HasHeaderFill { get; set; } = false;
        public ICollection<PartyModel>? Parties { get; set; } = [];
    }
}
