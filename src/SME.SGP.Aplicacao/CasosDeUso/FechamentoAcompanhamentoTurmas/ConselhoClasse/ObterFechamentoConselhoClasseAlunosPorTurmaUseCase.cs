﻿using MediatR;
using Newtonsoft.Json;
using Sentry;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterFechamentoConselhoClasseAlunosPorTurmaUseCase : AbstractUseCase, IObterFechamentoConselhoClasseAlunosPorTurmaUseCase
    {
        public ObterFechamentoConselhoClasseAlunosPorTurmaUseCase(IMediator mediator) : base(mediator)
        {
        }

        public async Task<IEnumerable<ConselhoClasseAlunoDto>> Executar(FiltroConselhoClasseConsolidadoTurmaBimestreDto param)
        {
            var turma = await mediator.Send(new ObterTurmaPorIdQuery(param.TurmaId));
            if (turma == null)
                throw new NegocioException("Turma não encontrada");

            var periodoEscolar = await mediator.Send(new ObterPeriodoEscolarPorTurmaBimestreQuery(turma, param.Bimestre));
            if (periodoEscolar == null)
                throw new NegocioException("Periodo escolar não encontrado");

            var alunos = await mediator.Send(new ObterEstudantesAtivosPorTurmaEDataReferenciaQuery(turma.CodigoTurma, periodoEscolar.PeriodoInicio));
            var consolidadoConselhosClasses = await mediator.Send(new ObterConselhoClasseConsolidadoPorTurmaBimestreQuery(turma.Id, param.Bimestre));
            alunos = alunos.Where(a => a.DataSituacao <= periodoEscolar.PeriodoFim).ToList();


            return await MontarRetorno(alunos, consolidadoConselhosClasses, turma.CodigoTurma);
        }

        private async Task<IEnumerable<ConselhoClasseAlunoDto>> MontarRetorno(IEnumerable<EstudanteDto> alunos, IEnumerable<ConselhoClasseConsolidadoTurmaAluno> consolidadoConselhosClasses, string codigoTurma)
        {
            List<ConselhoClasseAlunoDto> lista = new List<ConselhoClasseAlunoDto>();
            var pareceresConclusivos = await mediator.Send(new ObterPareceresConclusivosQuery());

            foreach (var aluno in alunos)
            {
                var consolidadoConselhoClasse = consolidadoConselhosClasses.FirstOrDefault(a => a.AlunoCodigo == aluno.CodigoAluno.ToString());
                var frequenciaGlobal = await mediator.Send(new ObterFrequenciaGeralAlunoQuery(aluno.CodigoAluno.ToString(), codigoTurma));
                string parecerConclusivo = consolidadoConselhoClasse.ParecerConclusivoId != null ? 
                    pareceresConclusivos.FirstOrDefault(a => a.Id == consolidadoConselhoClasse.ParecerConclusivoId).Nome : "";

                lista.Add(new ConselhoClasseAlunoDto()
                {
                    NumeroChamada = aluno.NumeroAlunoChamada,
                    AlunoCodigo = aluno.CodigoAluno.ToString(),
                    NomeAluno = aluno.NomeAluno,
                    SituacaoFechamento = consolidadoConselhoClasse.Status.Description(),
                    FrequenciaGlobal = frequenciaGlobal,
                    PodeExpandir = consolidadoConselhoClasse.Status != StatusFechamento.NaoIniciado,
                    ParecerConclusivo = parecerConclusivo
                });
            }

            return lista;

        }
    }
}
