﻿namespace DigitalBallotPlatform.Platform.DTOs
{
    public class LoginUserDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public double AuthInterval { get; set; }
        public bool IsRemembered { get; set; }
    }

    public class TokenRenewalRequest
    {
        public string Token { get; set; } = string.Empty;
        public double AuthInterval { get; set; }
    }
}
