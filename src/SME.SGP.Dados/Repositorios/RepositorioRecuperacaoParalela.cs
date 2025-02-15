﻿using Dapper;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioRecuperacaoParalela : RepositorioBase<RecuperacaoParalela>, IRepositorioRecuperacaoParalela
    {
        public RepositorioRecuperacaoParalela(ISgpContext conexao, IServicoAuditoria servicoAuditoria) : base(conexao, servicoAuditoria)
        {
        }

        public async Task<IEnumerable<RetornoRecuperacaoParalela>> Listar(long turmaId, long periodoId)
        {
            var query = new StringBuilder();
            query.AppendLine(MontaCamposCabecalho());
            query.AppendLine("from recuperacao_paralela rec");
            query.AppendLine("inner join recuperacao_paralela_periodo_objetivo_resposta recRel on rec.id = recRel.recuperacao_paralela_id");
            query.AppendLine("inner join recuperacao_paralela_resposta re on re.id = recRel.resposta_id");
            query.AppendLine("inner join recuperacao_paralela_periodo rpp on rpp.id = recRel.periodo_recuperacao_paralela_id");
            query.AppendLine("inner join turma t on rec.turma_id = t.id");
            query.AppendLine("inner join turma t2 on rec.turma_recuperacao_paralela_id = t2.id");
            query.AppendLine("where rec.turma_recuperacao_paralela_id = @turmaId ");
            query.AppendLine("and rec.excluido = false ");
            query.AppendLine("and rpp.id = @periodoId");
            return await database.Conexao.QueryAsync<RetornoRecuperacaoParalela>(query.ToString(), new { turmaId = turmaId, periodoId });
        }

        public async Task<IEnumerable<RetornoRecuperacaoParalelaTotalAlunosAnoDto>> ListarTotalAlunosSeries(int? periodoId, string dreId, string ueId, int? cicloId, string turmaId, 
            string ano, int anoLetivo)
        {
            
            var query = new StringBuilder();
            query.Append(@" select
                                count ( distinct aluno_id) as total,
                                turma.ano,
                                tipo_ciclo.descricao as Ciclo
                            from recuperacao_paralela rp
                                inner join turma on rp.turma_id = turma.id
                                inner join tipo_ciclo_ano tca on turma.modalidade_codigo = tca.modalidade and turma.ano = tca.ano
                                inner join tipo_ciclo on tca.tipo_ciclo_id = tipo_ciclo.id
                                inner join recuperacao_paralela_periodo_objetivo_resposta rpp on rp.id = rpp.recuperacao_paralela_id
                                inner join ue on ue.id = turma.ue_id
                                inner join dre on dre.id = ue.dre_id");

            MontarWhere(query, dreId, ueId, cicloId, ano, periodoId, turmaId, null);
            query.Append(@" group by
                                turma.ano,
                                tipo_ciclo.descricao");
            var parametros = new { dreId, ueId, cicloId, turmaId, ano, periodoId, anoLetivo };
            return await database.Conexao.QueryAsync<RetornoRecuperacaoParalelaTotalAlunosAnoDto>(query.ToString(), parametros);
        }

        public async Task<IEnumerable<RetornoRecuperacaoParalelaTotalAlunosAnoFrequenciaDto>> ListarTotalEstudantesPorFrequencia(int? periodoId, string dreId, string ueId, int? cicloId, 
            string turmaId, string ano, int anoLetivo)
        {
            var query = new StringBuilder();
            query.Append(@"select
	                            count(distinct aluno_id) as total,
	                            turma.ano,
	                            tipo_ciclo.descricao as Ciclo,
                                tipo_ciclo.id as CicloId,
                                rpr.id as RespostaId,
	                            rpr.nome as frequencia
                            from recuperacao_paralela rp
	                            inner join turma on rp.turma_id = turma.id
	                            inner join tipo_ciclo_ano tca on turma.modalidade_codigo = tca.modalidade and turma.ano = tca.ano
	                            inner join tipo_ciclo on tca.tipo_ciclo_id = tipo_ciclo.id
	                            inner join recuperacao_paralela_periodo_objetivo_resposta rpp on rp.id = rpp.recuperacao_paralela_id
	                            inner join recuperacao_paralela_resposta rpr on rpp.resposta_id = rpr.id
                                inner join  ue on ue.id = turma.ue_id
                                inner join dre on dre.id = ue.dre_id");
            MontarWhere(query, dreId, ueId, cicloId, ano, periodoId, turmaId, null);
            query.AppendLine("and rpp.objetivo_id = 4");
            query.Append(@"group by
	                            turma.nome,
	                            turma.ano,
	                            tipo_ciclo.descricao,
	                            rpr.nome,
                                rpr.id,
                                tipo_ciclo.id
                            order by rpr.ordem");

            var parametros = new { dreId, ueId, cicloId, turmaId, ano, periodoId, anoLetivo };

            return await database.Conexao.QueryAsync<RetornoRecuperacaoParalelaTotalAlunosAnoFrequenciaDto>(query.ToString(), parametros);
        }

        public async Task<PaginacaoResultadoDto<RetornoRecuperacaoParalelaTotalResultadoDto>> ListarTotalResultado(int? periodoId, string dreId, string ueId, int? cicloId, string turmaId, 
            string ano, int anoLetivo, int? pagina)
        {
            //a paginação desse ítem é diferente das outras, pois ela é determinada pela paginação da coluna pagina
            //ela não tem uma quantidade exata de ítens por página, apenas os objetivos daquele eixo, podendo variar para cada um
            if (pagina == 0) pagina = 1;
            StringBuilder query = new StringBuilder();
            query.AppendLine("select");
            MontarCamposResumo(query);
            MontarFromResumo(query);
            MontarWhere(query, dreId, ueId, cicloId, ano, periodoId, turmaId, pagina);
            query.AppendLine("and e.id NOT IN (1,2)");
            query.AppendLine("and e.excluido = false");
            query.AppendLine("and o.excluido = false");
            query.AppendLine("and rpr.excluido = false");
            query.AppendLine("group by");
            query.AppendLine("turma.nome,");
            query.AppendLine("turma.ano,");
            query.AppendLine("tipo_ciclo.id,");
            query.AppendLine("tipo_ciclo.descricao,");
            query.AppendLine("rpr.nome,");
            query.AppendLine("o.nome,");
            query.AppendLine("e.descricao,");
            query.AppendLine("o.ordem,");
            query.AppendLine("tipo_ciclo.descricao,");
            query.AppendLine("e.id,");
            query.AppendLine("o.id,");
            query.AppendLine("rpr.id");
            query.AppendLine("order by");
            query.AppendLine("o.ordem,");
            query.AppendLine("rpr.ordem;");
            query.AppendLine("select max(pagina) from recuperacao_paralela_objetivo;");

            var parametros = new { dreId, ueId, cicloId, turmaId, ano, periodoId, pagina, anoLetivo };
            var retorno = new PaginacaoResultadoDto<RetornoRecuperacaoParalelaTotalResultadoDto>();

            using (var multi = await database.Conexao.QueryMultipleAsync(query.ToString(), parametros))
            {
                retorno.Items = multi.Read<RetornoRecuperacaoParalelaTotalResultadoDto>().ToList();
                retorno.TotalRegistros = multi.ReadFirst<int>();
            }

            retorno.TotalPaginas = retorno.TotalRegistros;

            return retorno;
        }

        public async Task<IEnumerable<RetornoRecuperacaoParalelaTotalResultadoDto>> ListarTotalResultadoEncaminhamento(int? periodoId, string dreId, string ueId, int? cicloId, string turmaId, string ano, int anoLetivo, 
            int? pagina)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("select");
            MontarCamposResumo(query);
            MontarFromResumo(query);
            MontarWhere(query, dreId, ueId, cicloId, ano, periodoId, turmaId, pagina);
            query.AppendLine("and e.id = 1");
            query.AppendLine("and e.excluido = false");
            query.AppendLine("and o.excluido = false");
            query.AppendLine("and rpr.excluido = false");
            query.AppendLine("group by");
            query.AppendLine("turma.nome,");
            query.AppendLine("turma.ano,");
            query.AppendLine("tipo_ciclo.id,");
            query.AppendLine("tipo_ciclo.descricao,");
            query.AppendLine("rpr.nome,");
            query.AppendLine("o.nome,");
            query.AppendLine("e.descricao,");
            query.AppendLine("o.ordem,");
            query.AppendLine("tipo_ciclo.descricao,");
            query.AppendLine("e.id,");
            query.AppendLine("o.id,");
            query.AppendLine("rpr.id");
            query.AppendLine("order by");
            query.AppendLine("o.ordem,");
            query.AppendLine("rpr.ordem;");

            var parametros = new { dreId, ueId, cicloId, turmaId, ano, pagina, periodoId, anoLetivo };
            return await database.Conexao.QueryAsync<RetornoRecuperacaoParalelaTotalResultadoDto>(query.ToString(), parametros);
        }

        private static void MontarCamposResumo(StringBuilder query)
        {
            query.AppendLine("tipo_ciclo.descricao as ciclo,");
            query.AppendLine("count(aluno_id) as total,");
            query.AppendLine("tipo_ciclo.id as cicloId,");
            query.AppendLine("turma.ano,");
            query.AppendLine("tipo_ciclo.descricao,");
            query.AppendLine("rpr.nome as resposta,");
            query.AppendLine("o.descricao as objetivo,");
            query.AppendLine("e.descricao as eixo,");
            query.AppendLine("e.id as eixoId,");
            query.AppendLine("o.id as objetivoId,");
            query.AppendLine("rpr.id as respostaId,");
            query.AppendLine("rpr.ordem as ordem");
        }

        private static void MontarFromResumo(StringBuilder query)
        {
            query.AppendLine("from recuperacao_paralela rp");
            query.AppendLine("inner join turma on rp.turma_id = turma.id");
            query.AppendLine("inner join tipo_ciclo_ano tca on turma.modalidade_codigo = tca.modalidade and turma.ano = tca.ano");
            query.AppendLine("inner join tipo_ciclo on tca.tipo_ciclo_id = tipo_ciclo.id");
            query.AppendLine("inner join recuperacao_paralela_periodo_objetivo_resposta rpp on rp.id = rpp.recuperacao_paralela_id");
            query.AppendLine("inner join recuperacao_paralela_resposta rpr on rpp.resposta_id = rpr.id");
            query.AppendLine("inner join recuperacao_paralela_objetivo o on rpp.objetivo_id = o.id");
            query.AppendLine("inner join recuperacao_paralela_eixo e on o.eixo_id = e.id");
            query.AppendLine("inner join  ue on ue.id = turma.ue_id");
            query.AppendLine("inner join dre on dre.id = ue.dre_id");
        }

        private static void MontarWhere(StringBuilder query, string dreId, string ueId, int? cicloId, string ano, int? periodoId, string turmaId, int? pagina)
        {
            query.AppendLine(" where rp.excluido = false ");
            if (!string.IsNullOrEmpty(dreId) && dreId != "0")
                query.AppendLine(" and dre.dre_id = @dreId");
            if (!string.IsNullOrEmpty(ueId) && ueId != "0")
                query.AppendLine(" and ue.ue_id = @ueId");
            if (cicloId.NaoEhNulo() && cicloId != 0)
                query.AppendLine(" and tca.tipo_ciclo_id = @cicloId");
            if (!string.IsNullOrEmpty(ano) && ano != "0")
                query.AppendLine("and turma.ano = @ano");
            if (periodoId.NaoEhNulo() && periodoId != 0)
                query.AppendLine(" and rpp.periodo_recuperacao_paralela_id = @periodoId");
            if (!string.IsNullOrEmpty(turmaId) && turmaId != "0")
                query.AppendLine(" and turma.turma_id = @turmaId");
            if (pagina.HasValue)
                query.AppendLine(" and o.pagina = @pagina");

            query.AppendLine(" and rp.ano_letivo = @anoLetivo");

        }

        private string MontaCamposCabecalho()
        {
            return @"select
                        rec.id ,
	                    rec.aluno_id AS AlunoId,
	                    rec.turma_id AS TurmaId,
                        rec.criado_por,
                        rec.criado_em,
                        rec.criado_rf,
                        rec.alterado_por,
                        rec.alterado_em,
                        rec.alterado_rf,
	                    recRel.objetivo_id AS ObjetivoId,
	                    recRel.resposta_id AS RespostaId,
	                    recRel.periodo_recuperacao_paralela_id AS PeriodoRecuperacaoParalelaId,
                        re.ordem as OrdenacaoResposta,
                        rpp.bimestre_edicao as BimestreEdicao";
        }
    }
}