using Microsoft.AspNetCore.Mvc;
using WebApplication5.Dto;
using WebApplication5.Models;
using WebApplication5.Repository;

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TiersController : ControllerBase
    {
        private readonly IRepository<Tiers> _tiersRepository;

        public TiersController(IRepository<Tiers> tiersRepository)
        {
            _tiersRepository = tiersRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TiersDto>>> GetTiers()
        {
            var tiersList = await _tiersRepository.GetAllWithContactsAsync();
            if (tiersList == null) return NotFound();

            var result = tiersList.Select(t => new TiersDto
            {
                Id = t.Id,
                Matricule = t.Matricule,
                Nom = t.Nom,
                Adresse = t.Adresse,
                Ville = t.Ville,
                Delegation = t.Delegation,
                SecteurActiv = t.SecteurActiv,
                Tel = t.Tel,
                Cin = t.Cin,
                Statut = t.Statut,
                Contacts = t.Contacts.Select(c => new ContactsDto
                {
                    Id = c.Id,
                    Email = c.Email,
                    Phone = c.Phone
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TiersDto>> GetTiersById(int id)
        {
            var tiers = await _tiersRepository.GetTiersWithContactsAsync(id);
            if (tiers == null) return NotFound();

            var result = new TiersDto
            {
                Id = tiers.Id,
                Matricule = tiers.Matricule,
                Nom = tiers.Nom,
                Adresse = tiers.Adresse,
                Ville = tiers.Ville,
                Delegation = tiers.Delegation,
                SecteurActiv = tiers.SecteurActiv,
                Tel = tiers.Tel,
                Cin = tiers.Cin,
                Statut = tiers.Statut,
                Contacts = tiers.Contacts.Select(c => new ContactsDto
                {
                    Id = c.Id,
                    Email = c.Email,
                    Phone = c.Phone
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<TiersDto>> CreateTiers(TiersDto tiersDto)
        {
            var tiers = new Tiers
            {
                Matricule = tiersDto.Matricule,
                Nom = tiersDto.Nom,
                Adresse = tiersDto.Adresse,
                Ville = tiersDto.Ville,
                Delegation = tiersDto.Delegation,
                SecteurActiv = tiersDto.SecteurActiv,
                Tel = tiersDto.Tel,
                Cin = tiersDto.Cin,
                Statut = tiersDto.Statut,
                Contacts = tiersDto.Contacts.Select(c => new Contacts
                {
                    Email = c.Email,
                    Phone = c.Phone
                }).ToList()
            };

            var created = await _tiersRepository.CreateAsync(tiers);

            tiersDto.Id = created.Id;
            tiersDto.Contacts = created.Contacts.Select(c => new ContactsDto
            {
                Id = c.Id,
                Email = c.Email,
                Phone = c.Phone
            }).ToList();

            return CreatedAtAction(nameof(GetTiersById), new { id = tiersDto.Id }, tiersDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTiers(int id, TiersDto tiersDto)
        {
            var tiers = await _tiersRepository.GetTiersWithContactsAsync(id);
            if (tiers == null) return NotFound();

            tiers.Matricule = tiersDto.Matricule;
            tiers.Nom = tiersDto.Nom;
            tiers.Adresse = tiersDto.Adresse;
            tiers.Ville = tiersDto.Ville;
            tiers.Delegation = tiersDto.Delegation;
            tiers.SecteurActiv = tiersDto.SecteurActiv;
            tiers.Tel = tiersDto.Tel;
            tiers.Cin = tiersDto.Cin;
            tiers.Statut = tiersDto.Statut;

            var contactsToRemove = tiers.Contacts
                .Where(c => !tiersDto.Contacts.Any(dto => dto.Id == c.Id))
                .ToList();

            foreach (var contact in contactsToRemove)
                tiers.Contacts.Remove(contact);

            foreach (var contactDto in tiersDto.Contacts)
            {
                if (contactDto.Id == 0)
                {
                    tiers.Contacts.Add(new Contacts
                    {
                        Email = contactDto.Email,
                        Phone = contactDto.Phone
                    });
                }
                else
                {
                    var existing = tiers.Contacts.FirstOrDefault(c => c.Id == contactDto.Id);
                    if (existing != null)
                    {
                        existing.Email = contactDto.Email;
                        existing.Phone = contactDto.Phone;
                    }
                }
            }

            await _tiersRepository.UpdateAsync(tiers);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTiers(int id)
        {
            var tiers = await _tiersRepository.GetByIdAsync(id);
            if (tiers == null) return NotFound();

            await _tiersRepository.DeleteAsync(tiers);
            return NoContent();
        }
    }
}
