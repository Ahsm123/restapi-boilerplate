﻿using Api.Models;

namespace Api.Dtos.TokenDtos;

public class TokenDto
{
    public string TokenString { get; set; }
    public TokenType Type { get; set; }
    public DateTime Expires { get; set; }
    public bool Blacklisted { get; set; }
}
