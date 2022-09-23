using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.Setup;
using Xunit;

namespace SME.SGP.TesteIntegracao.Listao
{
    public class Ao_lancar_frequencia_professor : ListaoTesteBase
    {
        public Ao_lancar_frequencia_professor(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);

            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<VerificaPodePersistirTurmaDisciplinaEOLQuery, bool>),
                typeof(VerificaPodePersistirTurmaDisciplinaEOLQueryHandlerComPermissaoFake), ServiceLifetime.Scoped));
        }

        [Fact]
        public async Task Deve_lancar_frequencia_com_ausencia_remoto_e_presenca_para_os_alunos()
        {
            var filtroListao = new FiltroListao
            {
                Bimestre = 3,
                Modalidade = Modalidade.Fundamental,
                Perfil = ObterPerfilProfessor(),
                AnoTurma = ANO_8,
                TipoCalendario = ModalidadeTipoCalendario.FundamentalMedio,
                TipoTurma = TipoTurma.Regular,
                TurmaHistorica = false,
                ComponenteCurricularId = COMPONENTE_CURRICULAR_PORTUGUES_ID_138
            };

            await CriarDadosBasicos(filtroListao);

            var listaAulaId = ObterTodos<Dominio.Aula>().Select(c => c.Id).Distinct().ToList();
            listaAulaId.ShouldNotBeNull();

            var frequenciasSalvar = listaAulaId.Select(aulaId => new FrequenciaSalvarAulaAlunosDto
                { AulaId = aulaId, Alunos = ObterListaFrequenciaSalvarAluno() }).ToList();

            //-> Salvar a frequencia
            var useCaseSalvar = ServiceProvider.GetService<IInserirFrequenciaListaoUseCase>();
            useCaseSalvar.ShouldNotBeNull();
            await useCaseSalvar.Executar(frequenciasSalvar);
            
            //-> Obter os períodos de filtro
            var useCasePeriodos = ServiceProvider.GetService<IObterPeriodosPorComponenteUseCase>();
            useCasePeriodos.ShouldNotBeNull();
            var listaPeriodo = (await useCasePeriodos.Executar(TURMA_CODIGO_1, filtroListao.ComponenteCurricularId, true,
                filtroListao.Bimestre)).ToList();            

            //-> Obter retorno dos dados salvos e validar por período
            var useCaseObterFrequencia = ServiceProvider.GetService<IObterFrequenciasPorPeriodoUseCase>();
            useCaseObterFrequencia.ShouldNotBeNull();
            
            foreach (var periodo in listaPeriodo)
            {
                var filtroFrequenciaPorPeriodoDto = new FiltroFrequenciaPorPeriodoDto
                {
                    TurmaId = TURMA_CODIGO_1,
                    DisciplinaId = filtroListao.ComponenteCurricularId.ToString(),
                    ComponenteCurricularId = filtroListao.ComponenteCurricularId.ToString(),
                    DataInicio = periodo.DataInicio,
                    DataFim = periodo.DataFim
                };

                var frequenciasPorPeriodo = await useCaseObterFrequencia.Executar(filtroFrequenciaPorPeriodoDto);
                frequenciasPorPeriodo.ShouldNotBeNull();
                
                // TODO PAREI AQUI.
            }
        }

        private IEnumerable<FrequenciaSalvarAlunoDto> ObterListaFrequenciaSalvarAluno()
        {
            return CODIGOS_ALUNOS.Select(codigoAluno => new FrequenciaSalvarAlunoDto
                { CodigoAluno = codigoAluno, Frequencias = ObterFrequenciaAula(codigoAluno) }).ToList();
        }

        private IEnumerable<FrequenciaAulaDto> ObterFrequenciaAula(string codigoAluno)
        {
            string[] codigosAlunosAusencia = { CODIGO_ALUNO_1, CODIGO_ALUNO_3 };
            string[] codigosAlunosPresenca = { CODIGO_ALUNO_2, CODIGO_ALUNO_4, CODIGO_ALUNO_6 };
            string[] codigosAlunosRemotos = { CODIGO_ALUNO_5 };

            return QUANTIDADES_AULAS.Select(numeroAula => new FrequenciaAulaDto
            {
                NumeroAula = numeroAula,
                TipoFrequencia = codigosAlunosAusencia.Contains(codigoAluno) ? TipoFrequencia.F.ObterNomeCurto() :
                    codigosAlunosPresenca.Contains(codigoAluno) ? TipoFrequencia.C.ObterNomeCurto() :
                    codigosAlunosRemotos.Contains(codigoAluno) ? TipoFrequencia.R.ObterNomeCurto() :
                    TIPOS_FREQUENCIAS[new Random().Next(TIPOS_FREQUENCIAS.Length)].ObterNomeCurto()
            }).ToList();
        }
    }
}