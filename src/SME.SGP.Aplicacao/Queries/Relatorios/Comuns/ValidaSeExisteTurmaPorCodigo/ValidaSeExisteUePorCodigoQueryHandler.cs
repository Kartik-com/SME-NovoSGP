﻿using System;
using MediatR;
using SME.SGP.Dominio.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using SME.SGP.Dominio;

namespace SME.SGP.Aplicacao.Queries
{
    public class ValidaSeExisteUePorCodigoQueryHandler : IRequestHandler<ValidaSeExisteUePorCodigoQuery, bool>
    {
        private readonly IRepositorioUeConsulta repositorioUe;

        public ValidaSeExisteUePorCodigoQueryHandler(IRepositorioUeConsulta repositorioUe)
        {
            this.repositorioUe = repositorioUe ?? throw new ArgumentNullException(nameof(repositorioUe));
        }

        public async Task<bool> Handle(ValidaSeExisteUePorCodigoQuery request, CancellationToken cancellationToken)
        {
            if (repositorioUe.ObterPorCodigo(request.CodigoUe).EhNulo())
                throw new NegocioException("Não foi possível encontrar a UE");

            return true;
        }
    }
}
