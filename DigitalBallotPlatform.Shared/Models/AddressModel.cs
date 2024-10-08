﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalBallotPlatform.Shared.Models
{
    public class AddressModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Address1 { get; set; } = string.Empty;
        public string? Address2 { get; set; }

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public int Zipcode { get; set; }
        public bool IsSameAsBilling { get; set; }
        public string? ShpAddress1 { get; set; }
        public string? ShpAddress2 { get; set; }
        public string? ShpCity { get; set; }
        public string? ShpState { get; set; }
        public int? ShpZipcode { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public int? CompanyId { get; set; }
        public CompanyModel? Company { get; set; }
        [ForeignKey(nameof(CountyId))]
        public int? CountyId { get; set; }
        public CountyModel? County { get; set; }
    }
}
