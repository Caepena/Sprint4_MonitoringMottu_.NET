using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MonitoringMottu.API.Models;
using MonitoringMottu.Domain.Entities;
using MonitoringMottu.Domain.Interfaces;
using MonitoringMottu.Domain.Pagination;

namespace MonitoringMottu.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MotoController : ControllerBase
    {
        private readonly LinkGenerator _links;
        private readonly IMotoRepository _motosRepository;
        private readonly IRepository<Moto> _motoRepository;

        public MotoController(IRepository<Moto> motoRepository, LinkGenerator links, IMotoRepository motosRepository)
        {
            _motosRepository = motosRepository;
            _motoRepository = motoRepository;
            _links = links ?? throw new ArgumentException(nameof(links));
        }

        // GET: api/Moto
        [HttpGet]
        public async Task<IActionResult> GetMotos()
        {
            var motos = await _motoRepository.GetAllAsync();
            return Ok(motos);
        }

        // GET: api/Moto/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMoto(Guid id)
        {
            var moto = await _motoRepository.GetByIdAsync(id);
            if (moto == null)
                return NotFound("Moto não existe ou não foi encontrada");
            
            return Ok(moto);
        }

        // PUT: api/Moto/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PutMoto(Guid id, [FromBody] MotoInputModel inputModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var motoExiste = await _motoRepository.GetByIdAsync(id);
            if (motoExiste == null)
                return NotFound("Moto não existe ou não foi encontrada");
            
            try
            {
                motoExiste.Refresh(
                    modelo: inputModel.Modelo,
                    marca: inputModel.Marca,
                    cor: inputModel.Cor,
                    placa: inputModel.Placa
                    );
                
                await _motoRepository.UpdateAsync(motoExiste);
                await _motoRepository.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Erro ao atualizar o Moto do Produto: {e.Message}");
            }
        }

        // POST: api/Moto
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostMoto([FromBody] MotoInputModel inputModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var moto = new Moto(
                    modelo: inputModel.Modelo,
                    marca: inputModel.Marca,
                    cor: inputModel.Cor,
                    placa: inputModel.Placa,
                    garagemId: inputModel.GaragemId
                );

                await _motoRepository.AddAsync(moto);
                await _motoRepository.SaveChangesAsync();
                
                return CreatedAtAction(nameof(GetMoto), new { id = moto.Id }, moto);
            }
            catch (Exception e)
            {
                return BadRequest($"Erro ao cadastrar moto: {e.Message}");
            }
        }

        // DELETE: api/Moto/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMoto(Guid id)
        {
            var moto = await _motoRepository.GetByIdAsync(id);
            if (moto == null)
            {
                return NotFound("Moto não existe ou não foi encontrada");
            }

            try
            {
                await _motoRepository.DeleteAsync(id);
                await _motoRepository.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest($"Erro ao deletar o Moto: {e.Message}");
            }
        }
        
        
        [HttpGet("paginado", Name = "GetMotosPaged")]
        [Produces("application/hal+json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? sortDir = "Desc",
            CancellationToken ct = default)
        {
            var pr = await _motosRepository.GetPaginationAsyncMoto(page, pageSize, ct);
            pr ??= new PageResult<Moto> { Items = Array.Empty<Moto>(), Page = page, PageSize = pageSize, Total = 0 };

            var items = pr.Items.Select(m => new Moto.MotoResponse(
                m.Id,
                m.Placa,
                m.Modelo,
                m.Cor,
                m.Placa,
                m.StatusMoto.ToString(),
                m.GaragemId
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
                    "GetMotosPaged",
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
                _embedded = new { motos = items },
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
