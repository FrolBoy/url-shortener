﻿using LongToShortUrl.Data.Models;
using LongToShortUrl.Data.Repo;
using LongToShortUrl.Services.Interfaces;

namespace LongToShortUrl.Services;

public class UrlShorteningService : IUrlShorteningService
{
    private readonly IUrlRepository _urlRepository;
    private readonly IUrlConversionAlgorithm _urlConversionAlgorithm;

    public UrlShorteningService(IUrlRepository urlRepository, IUrlConversionAlgorithm urlConversionAlgorithm)
    {
        _urlRepository = urlRepository;
        _urlConversionAlgorithm = urlConversionAlgorithm;
    }

    public async Task<Url> CreateShortUrlAsync(string longUrl)
    {
        if (!Uri.TryCreate(longUrl, UriKind.Absolute, out var uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Invalid URL");
        }

        var existingUrl = await _urlRepository.GetByUrlAsync(longUrl);
        if (existingUrl != null)
        {
            return existingUrl;
        }

        var urlCode = _urlConversionAlgorithm.GenerateUrlCode(longUrl);
        var shortUrl = new Url
        {
            LongUrl = longUrl,
            UrlCode = urlCode,
            ShortUrl = $"{GlobalConstants.BaseUrl}/{urlCode}",
            CreationDate = DateTime.UtcNow
        };

        await _urlRepository.AddUrlAsync(shortUrl);
        await _urlRepository.SaveChangesAsync();

        return shortUrl;
    }

    public async Task<Url> GetLongUrlAsync(string shortUrlCode)
    {
        var url = await _urlRepository.GetByCodeAsync(shortUrlCode);
        return url;
    }
}