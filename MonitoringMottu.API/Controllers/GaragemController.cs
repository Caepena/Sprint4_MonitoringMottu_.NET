using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MonitoringMottu.API.Models;
using MonitoringMottu.Domain;
using MonitoringMottu.Domain.Entities;
using MonitoringMottu.Domain.Interfaces;
using MonitoringMottu.Domain.Pagination;
using MonitoringMottu.Infrastructure.Context;

namespace MonitoringMottu.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GaragemController : ControllerBase
    {
        private readonly LinkGenerator _links;
        private readonly IRepository<Garagem> _garagemRepository;
        private readonly IGaragemRepository _garagensRepository;

        public GaragemController(IRepository<Garagem> garagemRepository, LinkGenerator links, IGaragemRepository garagemsRepository)
        {
            _garagemRepository = garagemRepository;
            _garagensRepository = garagemsRepository;
            _links = links ?? throw new ArgumentException(nameof(links));
        }

        // GET: api/Garagem
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Garagem>>> GetGaragens()
        {
            var garagens = await _garagemRepository.GetAllAsync();
            return Ok(garagens);
        }


        // GET: api/Garagem/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGaragem(Guid id)
        {
            var garagem = await _garagemRepository.GetByIdAsync(id);
            if (garagem == null)
                return NotFound("Garagem não existe ou não foi encontrada");
            
            return Ok(garagem);
        }

        // PUT: api/Garagem/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PutGaragem (Guid id, [FromBody] GaragemInputModel inputModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var garagemExiste = await _garagemRepository.GetByIdAsync(id);
            if (garagemExiste == null)
                return NotFound("Garagem não existe ou não foi encontrada");
            
            try
            {
                garagemExiste.Refresh(
                    endereco: inputModel.Endereco,
                    responsavel: inputModel.Responsavel,
                    capacidade: inputModel.Capacidade
                    );
                
                await _garagemRepository.UpdateAsync(garagemExiste);
                await _garagemRepository.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Erro ao atualizar a Garagem do Produto: {e.Message}");
            }
        }

        // POST: api/Garagem
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostGaragem([FromBody] GaragemInputModel inputModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var garagem = new Garagem(
                    endereco: inputModel.Endereco,
                    capacidade: inputModel.Capacidade,
                    responsavel: inputModel.Responsavel
                );

                await _garagemRepository.AddAsync(garagem);
                await _garagemRepository.SaveChangesAsync();
                
                return CreatedAtAction(nameof(GetGaragem), new { id = garagem.Id }, garagem);
            }
            catch (Exception e)
            {
                return BadRequest($"Erro ao cadastrar Garagem: {e.Message}");
            }
        }

        // DELETE: api/Garagem/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGaragem(Guid id)
        {
            var garagem = await _garagemRepository.GetByIdAsync(id);
            if (garagem == null)
            {
                return NotFound("Garagem não existe ou não foi encontrada");
            }

            try
            {
                await _garagemRepository.DeleteAsync(id);
                await _garagemRepository.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest($"Erro ao deletar a garagem: {e.Message}");
            }
        }
        
        [HttpGet("paginado", Name = "GetGaragensPaged")]
        [Produces("application/hal+json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? sortDir = "Desc",
            CancellationToken ct = default)
        {
            var pr = await _garagensRepository.GetPaginationAsyncGaragem(page, pageSize, ct);
            pr ??= new PageResult<Garagem> { Items = Array.Empty<Garagem>(), Page = page, PageSize = pageSize, Total = 0 };

            var items = pr.Items.Select(g => new Garagem.GaragemResponse(
                g.Id,
                g.Endereco,
                g.Capacidade,
                g.Responsavel
            )).ToList();
            
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var total = pr.Total;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;

            var selfPage = Math.Clamp(page, 1, totalPages);
            
            string? LinkTo(int targetPage)
            {
                return _links.GetUriByName(
                    HttpContext,
                    "GetGaragensPaged",
                    values: new
                    {
                        page = targetPage,
                        pageSize,
                        search,
                        sortDir
                    });
            }
            
            var linkSelf  = LinkTo(selfPage);
            var linkFirst = LinkTo(1);
            var linkLast  = LinkTo(totalPages);
            var linkPrev  = selfPage > 1          ? LinkTo(selfPage - 1) : null;
            var linkNext  = selfPage < totalPages ? LinkTo(selfPage + 1) : null;

            var links = new Dictionary<string, object>();
            if (linkSelf  is not null) links["self"]  = new { href = linkSelf  };
            if (linkFirst is not null) links["first"] = new { href = linkFirst };
            if (linkPrev  is not null) links["prev"]  = new { href = linkPrev  };
            if (linkNext  is not null) links["next"]  = new { href = linkNext  };
            if (linkLast  is not null) links["last"]  = new { href = linkLast  };

            var body = new
            {
                _embedded = new { garagens = items },
                _links = links,
                page = new
                {
                    size = pageSize,
                    totalElements = total,
                    totalPages,
                    number = selfPage - 1 // zero-based
                }
            };

            return Ok(body);
        }
    }
}
