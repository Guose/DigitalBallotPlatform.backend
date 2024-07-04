﻿using DigitalBallotPlatform.Election.Models;
using DigitalBallotPlatform.Shared.Models;
using System.Globalization;

namespace DigitalBallotPlatform.Election.DTOs
{
    public class ElectionSetupDTO
    {
        private string? _formattedDate;

        public int Id { get; set; }
        public DateTime ElectionDate { get; set; }
        public string? Description { get; set; }
        public int WatermarkId { get; set; }
        public int CountyId { get; set; }
        public int BallotSpecsId { get; set; }
        public List<PartyDTO>? Parties { get; set; }

        public string FormattedDate
        {
            get => _formattedDate!;
            set
            {
                if (DateTime.TryParseExact(value, "yyyyMMdd", null, DateTimeStyles.None, out DateTime parsedDate))
                {
                    _formattedDate = value;
                    ElectionDate = parsedDate;
                }
                else
                {
                    throw new ArgumentException("Invalid date format. Expected: yyyyMMdd");
                }
            }
        }

        public ElectionSetupDTO() { }
        public ElectionSetupDTO(int id, DateTime electionDate, string description, int watermarkId, int countyId, int ballotSpecsId, List<PartyDTO> parties)
        {
            Id = id;
            ElectionDate = electionDate;
            Description = description;
            WatermarkId = watermarkId;
            CountyId = countyId;
            BallotSpecsId = ballotSpecsId;
            Parties = parties;
        }

        public static ElectionSetupDTO MapElectionSetupEntity(ElectionSetupModel electionSetup)
        {
            return new ElectionSetupDTO
            {
                Id = electionSetup.Id,
                ElectionDate = electionSetup.ElectionDate,
                Description = electionSetup.Description,
                WatermarkId = electionSetup.WatermarkId,
                CountyId = electionSetup.CountyId,
                BallotSpecsId = electionSetup.BallotSpecsId,
                // Parties = [.. electionSetup.Parties]
            };
        }

        public static ElectionSetupModel MapElectionSetupDTO(ElectionSetupDTO electionSetupDTO)
        {
            return new ElectionSetupModel
            {
                Id = electionSetupDTO.Id,
                ElectionDate = electionSetupDTO.ElectionDate,
                Description = electionSetupDTO.Description,
                WatermarkId = electionSetupDTO.WatermarkId,
                CountyId = electionSetupDTO.CountyId,
                BallotSpecsId = electionSetupDTO.BallotSpecsId,
                // Parties = [.. electionSetupDTO.Parties]
            };
        }
    }
}
