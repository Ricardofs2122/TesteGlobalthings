## TesteGlobalthings
Parte 01: 

a) Resposta: Criaria uma API com um endpoint para recebimento de medições em lote (batch),  "POST /medicoes" e uma chave unica entre ID e DataHoraMedicao para evitar duplicidade no banco.  

Justificativa:
Envio em Lote: Como o firmware pode acumular dados quando o WiFi está intermitente, a API precisa aceitar várias medições no mesmo request

Chave Única: Caso os sensores dem algum problema ou reenviem dados duplicados é possivel garantir que os dados não foram duplicados.

b)Resposta: A API deve receber uma lista de Medições , pois o sensor pode enviar dados acumulados ou não. Para isso foram criados dois DTOs de Request. 

Um com todos os dados que irão persistir no banco e cum com o List que ira receber os dados do FromBody
```
public class MedicoesDTO
{
  public int Id { get; set; }
  public string Codigo { get; set; } = string.Empty;
  public DateTimeOffset DataHoraMedicao { get; set; }
  public decimal Medicao { get; set; }
}

public class MedicoesBatchDTO
{
  public List<MedicoesDTO> Medicoes { get; set; } = new();
}

```

E temos o request e Persistência em banco de dados tambem trazendo quandos dados não foram gravados e quanto deram falha.

```
[HttpPost]
public async Task<IActionResult> Medicoes([FromBody] MedicoesBatchDTO request)

{

    var response = new MedicaoBachResultado
    {
        TotalRecebidas = request.Medicoes?.Count ?? 0
    };

    if (request.Medicoes == null || !request.Medicoes.Any())
        return BadRequest(response);

    using var transaction = await _context.Database.BeginTransactionAsync();

    foreach (var dto in request.Medicoes)
    {
        var resultado = new MedicaoResultadoDTO
        {
            Id = dto.Id,
            DataHoraMedicao = dto.DataHoraMedicao
        };
        
        
        if (string.IsNullOrWhiteSpace(dto.Codigo))
        {
            resultado.Sucesso = false;
            resultado.Mensagem = "Código inválido.";
            response.Resultados.Add(resultado);
            continue;
        }

        if (dto.DataHoraMedicao == default)
        {
            resultado.Sucesso = false;
            resultado.Mensagem = "DataHoraMedicao inválida.";
            response.Resultados.Add(resultado);
            continue;
        }

        var medicao = new Medicoes
        {
            Id = dto.Id,
            Codigo = dto.Codigo,
            DataHoraMedicao = dto.DataHoraMedicao,
            Medicao = dto.Medicao
        };

        try
        {
            await _context.Medicoes.AddAsync(medicao);
            await _context.SaveChangesAsync();

            resultado.Sucesso = true;
            resultado.Mensagem = "Medição salva com sucesso.";
            response.TotalProcessadas++;
        }
        catch (DbUpdateException)
        {
            resultado.Sucesso = false;
            resultado.Mensagem = "Medição duplicada, ignorada.";
        }

        response.Resultados.Add(resultado);
    }

    await transaction.CommitAsync();

    return Ok(response);

}
```

c) Resposta: Na minha opinião e também pela minha experiência em baco de dados usaria um banco de dados relacional como SQLServer, Postgre ou Mysql "Que é o que está no exemplo que criei", por ser mais estruturada e tipos bem definidos, consistência e integridade dos dados, sem contar o suporte ao DateTimeOffset.

  
