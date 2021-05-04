import PropTypes from 'prop-types';
import React, { useState } from 'react';
import { Base } from '~/componentes';
import CardCollapse from '~/componentes/cardCollapse';
import GraficoFrequenciaGlobalPorDRE from './graficoFrequenciaGlobalPorDRE';

const FrequenciaGlobalPorDRE = props => {
  const {
    anoLetivo,
    dreId,
    ueId,
    modalidade,
    semestre,
    listaAnosEscolares,
  } = props;

  const configCabecalho = {
    altura: '44px',
    corBorda: Base.AzulBordaCollapse,
  };

  const [exibir, setExibir] = useState(false);

  const key = 'Frequencia-global-por-dre';

  return (
    <div className="mt-3">
      <CardCollapse
        titulo="Frequência global por DRE"
        key={`${key}-collapse-key`}
        indice={`${key}-collapse-indice`}
        alt={`${key}-alt`}
        configCabecalho={configCabecalho}
        show={exibir}
        onClick={() => {
          setExibir(!exibir);
        }}
      >
        {exibir ? (
          <GraficoFrequenciaGlobalPorDRE
            anoLetivo={anoLetivo}
            dreId={dreId}
            ueId={ueId}
            modalidade={modalidade}
            semestre={semestre}
            listaAnosEscolares={listaAnosEscolares}
          />
        ) : (
          ''
        )}
      </CardCollapse>
    </div>
  );
};

FrequenciaGlobalPorDRE.propTypes = {
  anoLetivo: PropTypes.oneOfType(PropTypes.any),
  dreId: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  ueId: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  modalidade: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  semestre: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  listaAnosEscolares: PropTypes.oneOfType(PropTypes.array),
};

FrequenciaGlobalPorDRE.defaultProps = {
  anoLetivo: null,
  dreId: null,
  ueId: null,
  modalidade: null,
  semestre: null,
  listaAnosEscolares: [],
};

export default FrequenciaGlobalPorDRE;
