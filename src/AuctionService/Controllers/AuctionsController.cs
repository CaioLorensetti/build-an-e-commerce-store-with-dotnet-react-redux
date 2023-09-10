using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuctionDto>>> GetAllAuctions()
    {
        var auctions = await _context.Auctions
            .Include(_ => _.Item)
            .OrderBy(_ => _.Item.Make)
            .ToListAsync();
        return Ok(_mapper.Map<IEnumerable<AuctionDto>>(auctions));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(_ => _.Item)
            .FirstOrDefaultAsync(_ => _.Id == id);
        if (auction == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<AuctionDto>(auction));
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        // TODO: add current user as seller
        auction.Seller = "test";
        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Could not save changes.");
        }
        return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AuctionDto>> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions
                            .Include(_ => _.Item)
                            .FirstOrDefaultAsync(_ => _.Id == id);
        if (auction == null)
        {
            return NotFound();
        }

        // TODO: add seller = username

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;

        //_mapper.Map(updateAuctionDto, auction);
        _context.Auctions.Update(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Could not save changes.");
        }
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null)
        {
            return NotFound();
        }

        // TODO: check seller = username

        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Could not save changes.");
        }
        return Ok();
    }
}
