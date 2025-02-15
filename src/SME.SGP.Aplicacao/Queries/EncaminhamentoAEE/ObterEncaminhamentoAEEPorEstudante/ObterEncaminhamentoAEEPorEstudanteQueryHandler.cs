﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterEncaminhamentoAEEPorEstudanteQueryHandler : IRequestHandler<ObterEncaminhamentoAEEPorEstudanteQuery, EncaminhamentoAEEResumoDto>
    {
        private readonly IRepositorioEncaminhamentoAEE repositorioEncaminhamentoAEE;

        public ObterEncaminhamentoAEEPorEstudanteQueryHandler(IRepositorioEncaminhamentoAEE repositorioEncaminhamentoAEE)
        {
            this.repositorioEncaminhamentoAEE = repositorioEncaminhamentoAEE ?? throw new ArgumentNullException(nameof(repositorioEncaminhamentoAEE));
        }

        public async Task<EncaminhamentoAEEResumoDto> Handle(ObterEncaminhamentoAEEPorEstudanteQuery request, CancellationToken cancellationToken)
        {
            var encaminhamento = await repositorioEncaminhamentoAEE.ObterEncaminhamentoPorEstudante(request.EstudanteCodigo, request.UeCodigo);

            return MapearParaDto(encaminhamento);
        }

        private EncaminhamentoAEEResumoDto MapearParaDto(EncaminhamentoAEEAlunoTurmaDto encaminhamento)
            => encaminhamento.EhNulo() ? null :
            new EncaminhamentoAEEResumoDto()
            {
                Id = encaminhamento.Id,
                Situacao = encaminhamento.Situacao != 0 ? encaminhamento.Situacao.Name() : "",
                SituacaoTipo = encaminhamento.Situacao,
                Turma = $"{encaminhamento.TurmaModalidade.ShortName()} - {encaminhamento.TurmaNome}"
            };
    }
}
