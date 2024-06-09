using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using prova2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace prova2.Controllers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
[Authorize]
public class PessoasController : BaseController
{
    private readonly ILogger<Controller> _logger;
    private readonly IWebHostEnvironment _appEnvironment;

    public PessoasController(ILogger<PessoasController> logger, IWebHostEnvironment appEnvironment)
    {
        _logger = logger;
        _appEnvironment = appEnvironment;
    }

    [HttpGet]
    public IActionResult Pessoa(int id = 0)
    {
        Repositorio<Pessoa> repo = new Repositorio<Pessoa>();
        Pessoa Pessoa = repo.Buscar(id);


        return View(Pessoa);
    }

    public IActionResult Pessoa()
    {
        Repositorio<Pessoa> repoPessoa = new Repositorio<Pessoa>();
        var pessoas = repoPessoa.Listar();
        return View(pessoas);
    }

    public IActionResult Listar(string MensagemErro = null)
    {
        Repositorio<Pessoa> repo = new Repositorio<Pessoa>();
        List<Pessoa> pessoas = repo.Listar();
        ViewBag.MensagemErro = MensagemErro;
        
        return View(pessoas);
    }

    [HttpPost]
    public IActionResult Pessoa(Pessoa model, IFormFile anexo)
    {   
        string caminho = null;
        if (anexo != null && anexo.Length > 0)
        {
            caminho = _appEnvironment.WebRootPath + "//imagens//" + anexo.FileName;
            using(FileStream stream = new FileStream(caminho, FileMode.Create))
            {
                anexo.CopyTo(stream);
            }
            caminho = "//imagens//" + anexo.FileName;
        }
        model.Imagem = caminho;
        Repositorio<Pessoa> repo = new Repositorio<Pessoa>();

        List<Pessoa> pessoas = repo.Listar();
        var pessoaCodigoFiscalExistente = pessoas.FirstOrDefault(p => p.CodigoFiscal.Contains(model.CodigoFiscal));
        var pessoaInscricaoEstadualExistente = pessoas.FirstOrDefault(p => p.InscricaoEstadual.Contains(model.InscricaoEstadual));

        if (pessoaCodigoFiscalExistente != null) {
            ViewBag.Errors = "Código Fiscal já existe";
            return View(model);
        }

        if (pessoaInscricaoEstadualExistente != null) {
            ViewBag.Errors = "Inscrição Estadual já existe";
            return View(model);
        }

        if (model.InscricaoEstadual.Length > 15)
        {
            ViewBag.Errors = "Inscrição Estadual deve ter até 15 caracteres";
            return View(model);
        }

        if (model.Id != null && model.Id != 0) {
            repo.Atualizar(model);
        } else {
            try {
                repo.Adicionar(model);
            } catch (Exception ex) {
                _logger.LogError("Erro", ex);
                throw ex;
            }
        }
        return RedirectToAction("Listar");
    }

    public FileContentResult Exportar(int id) {
        Repositorio<Pessoa> repo = new Repositorio<Pessoa>();
        Pessoa model = repo.Buscar(id);
        string json = JsonSerializer.Serialize(model);

        return File(new System.Text.UTF8Encoding().GetBytes(json), "text/json", "Dados Json");
    }

    public IActionResult Apagar(int id)
    {
        Repositorio<Pessoa> repo = new Repositorio<Pessoa>();
        Pessoa pessoa = repo.Buscar(id);
        
        if (pessoa == null)
        {
            return NotFound();
        }

        return View(pessoa);
    }

    [HttpPost]
    public IActionResult Apagar(Pessoa pessoa)
    {
        Repositorio<Pessoa> repo = new Repositorio<Pessoa>();
        repo.Remover(pessoa.Id);
        return RedirectToAction("Listar");
    }
}
