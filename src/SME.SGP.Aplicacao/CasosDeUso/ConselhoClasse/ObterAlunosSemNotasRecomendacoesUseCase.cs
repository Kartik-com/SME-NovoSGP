﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Constantes.MensagensNegocio;
using SME.SGP.Infra;

namespace SME.SGP.Aplicacao
{
    public class ObterAlunosSemNotasRecomendacoesUseCase : IObterAlunosSemNotasRecomendacoesUseCase
    {
        private readonly IMediator mediator;

        public ObterAlunosSemNotasRecomendacoesUseCase(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<InconsistenciasAlunoFamiliaDto>> Executar(FiltroInconsistenciasAlunoFamiliaDto param)
        {
            var turmaRegular = await mediator.Send(new ObterTurmaPorIdQuery(param.TurmaId));
            if (turmaRegular == null)
                throw new NegocioException(MensagemNegocioTurma.TURMA_NAO_ENCONTRADA);

            var retorno = new List<InconsistenciasAlunoFamiliaDto>();
            var turmasCodigo = new List<string>();
            var periodoEscolar = await ObterPeriodoEscolar(turmaRegular, param.Bimestre);

            turmasCodigo.Add(turmaRegular.CodigoTurma);
            var alunosDaTurma = await mediator.Send(new ObterTodosAlunosNaTurmaQuery(int.Parse(turmaRegular.CodigoTurma)));

            var turmaComplementares = await mediator.Send(new ObterTurmasComplementaresPorAlunoQuery(alunosDaTurma.Select(x => x.CodigoAluno).ToArray()));
            if (turmaComplementares.Any())
                turmasCodigo.AddRange(turmaComplementares.Select(x => x.CodigoTurma));

            var turmasItinerarioEnsinoMedio = await mediator.Send(new ObterTurmaItinerarioEnsinoMedioQuery());

            var codigosItinerarioEnsinoMedio = await ObterTurmasCodigosItinerarioEnsinoMedio(turmaRegular, turmasItinerarioEnsinoMedio, periodoEscolar, param.Bimestre);
            if (codigosItinerarioEnsinoMedio != null)
                turmasCodigo.AddRange(codigosItinerarioEnsinoMedio);

            var usuarioLogado = await mediator.Send(new ObterUsuarioLogadoQuery());
            var perfil = await mediator.Send(new ObterPerfilAtualQuery());

            var componentesCurricularesPorTurma = await mediator.Send(new ObterComponentesCurricularesPorTurmasCodigoQuery(turmasCodigo.ToArray(), perfil, usuarioLogado.CodigoRf, turmaRegular.EnsinoEspecial, turmaRegular.TurnoParaComponentesCurriculares));

            var obterRecomendacoes = await mediator.Send(new VerificarSeExisteRecomendacaoPorTurmaQuery(componentesCurricularesPorTurma.Select(x => x.TurmaCodigo).ToArray(), param.Bimestre));

            var obterConselhoClasseAlunoNota = await mediator.Send(new ObterConselhoClasseAlunoNotaQuery(componentesCurricularesPorTurma.Select(x => x.TurmaCodigo).ToArray(), param.Bimestre));
            MapearRetorno(retorno,obterRecomendacoes,obterConselhoClasseAlunoNota,alunosDaTurma,componentesCurricularesPorTurma);
            return retorno;
        }

        private void MapearRetorno(List<InconsistenciasAlunoFamiliaDto> retorno, IEnumerable<AlunoTemRecomandacaoDto> obterRecomendacoes, IEnumerable<ConselhoClasseAlunoNotaDto> obterConselhoClasseAlunoNota, IEnumerable<AlunoPorTurmaResposta> alunoPorTurmaRespostas, 
            IEnumerable<DisciplinaDto> componentesCurricularesPorTurma)
        {
            foreach (var aluno in alunoPorTurmaRespostas)
            {
                var item = new InconsistenciasAlunoFamiliaDto
                {
                    NumeroChamada = aluno.NumeroAlunoChamada,
                    AlunoNome = aluno.NomeAluno,
                    AlunoCodigo = aluno.CodigoAluno
                };
                foreach (var componente in componentesCurricularesPorTurma)
                {
                    var componentetemNota = obterConselhoClasseAlunoNota.Where(c => c.ComponenteCurricularId == componente.Id);
                    if(!componentetemNota.Any())
                        item.Inconsistencias.Add(string.Format(MensagemNegocioConselhoClasse.AUSENCIA_DA_NOTA_NO_COMPONENTE,componente.Nome));
                }

                var existeRecomendacao = obterRecomendacoes.Where(x => x.AluncoCodigo == aluno.CodigoAluno);
                if(!existeRecomendacao.Any() )
                    item.Inconsistencias.Add(MensagemNegocioConselhoClasse.SEM_RECOMENDACAO_FAMILIA_ESTUDANDE);
                
                retorno.Add(item);
            }
        }

        private async Task<PeriodoEscolar> ObterPeriodoEscolar(Turma turma, int bimestre)
        {
            var fechamentoDaTurma = await mediator.Send(new ObterFechamentoTurmaPorIdTurmaQuery(turma.Id, bimestre));
            if (fechamentoDaTurma != null)
                return fechamentoDaTurma?.PeriodoEscolar;
            else return await mediator.Send(new ObterPeriodoEscolarAtualQuery(turma.Id, DateTime.Now.Date));
        }

        private async Task<string[]> ObterTurmasCodigosItinerarioEnsinoMedio(Turma turma, IEnumerable<TurmaItinerarioEnsinoMedioDto> turmasItinerarioEnsinoMedio, PeriodoEscolar periodoEscolar, int bimestre)
        {
            string[] turmasCodigos = null;
            if ((turma.DeveVerificarRegraRegulares() || turmasItinerarioEnsinoMedio.Any(a => a.Id == (int) turma.TipoTurma))
                && !(bimestre == 0 && turma.EhEJA() && !turma.EhTurmaRegular()))
            {
                var ue = await mediator.Send(new ObterUePorIdQuery(turma.UeId));
                var tiposParaConsulta = new List<int> {(int) turma.TipoTurma};
                var tiposRegularesDiferentes = turma.ObterTiposRegularesDiferentes();
                tiposParaConsulta.AddRange(tiposRegularesDiferentes.Where(c => tiposParaConsulta.All(x => x != c)));
                tiposParaConsulta.AddRange(turmasItinerarioEnsinoMedio.Select(s => s.Id).Where(c => tiposParaConsulta.All(x => x != c)));
                turmasCodigos = await mediator.Send(new ObterTurmaCodigosAlunoPorAnoLetivoUeTipoTurmaQuery(turma.AnoLetivo, tiposParaConsulta, false, ue.CodigoUe, turma.Semestre, periodoEscolar.PeriodoFim));
                if (!turmasCodigos.Any())
                    turmasCodigos = new string[1] {turma.CodigoTurma};
                else if (!turmasCodigos.Contains(turma.CodigoTurma))
                    turmasCodigos = turmasCodigos.Concat(new[] {turma.CodigoTurma}).ToArray();
            }
            else turmasCodigos = new string[] {turma.CodigoTurma};

            return turmasCodigos;
        }
    }
}