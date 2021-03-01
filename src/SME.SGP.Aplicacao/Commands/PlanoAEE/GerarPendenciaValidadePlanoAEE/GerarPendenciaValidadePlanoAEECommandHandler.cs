﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class GerarPendenciaValidadePlanoAEECommandHandler : IRequestHandler<GerarPendenciaValidadePlanoAEECommand, bool>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMediator mediator;
        private readonly IRepositorioPendenciaPlanoAEE repositorioPendenciaPlanoAEE;

        public GerarPendenciaValidadePlanoAEECommandHandler(IUnitOfWork unitOfWork, IMediator mediator, IRepositorioPendenciaPlanoAEE repositorioPendenciaPlanoAEE)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.repositorioPendenciaPlanoAEE = repositorioPendenciaPlanoAEE ?? throw new ArgumentNullException(nameof(repositorioPendenciaPlanoAEE));
        }

        public async Task<bool> Handle(GerarPendenciaValidadePlanoAEECommand request, CancellationToken cancellationToken)
        {
            using (var transacao = unitOfWork.IniciarTransacao())
            {
                try
                {
                    var pendenciaId = await mediator.Send(new SalvarPendenciaCommand(TipoPendencia.AEE, request.Descricao, titulo: request.Titulo));

                    await repositorioPendenciaPlanoAEE.SalvarAsync(new PendenciaPlanoAEE(pendenciaId, request.PlanoAEEId));

                    await mediator.Send(new SalvarPendenciaUsuarioCommand(pendenciaId, request.UsuarioId));

                    unitOfWork.PersistirTransacao();
                }
                catch (Exception e)
                {
                    unitOfWork.Rollback();
                    throw;
                }            
            }

            return true;
        }
    }
}
