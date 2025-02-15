﻿using Dapper;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Interface;
using System.Threading.Tasks;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioItineranciaObjetivo : RepositorioBase<ItineranciaObjetivo>, IRepositorioItineranciaObjetivo
    {
        public RepositorioItineranciaObjetivo(ISgpContext database, IServicoAuditoria servicoAuditoria) : base(database, servicoAuditoria)
        {
        }

        public async Task ExcluirItineranciaObjetivo(long objetivoId, long itineranciaId)
        {
            await database.Conexao.ExecuteScalarAsync(@"delete from itinerancia_objetivo iq where itinerancia_id = @itineranciaId and id = @objetivoId", new { objetivoId, itineranciaId });
        }
    }
}
