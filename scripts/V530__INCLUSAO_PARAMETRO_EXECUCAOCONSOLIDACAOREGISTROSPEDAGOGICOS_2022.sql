-- BUG 59651

UPDATE public.parametros_sistema set ativo = false, alterado_em = null, valor = '' where tipo = 82 and ano = 2021;

INSERT INTO public.parametros_sistema
    (nome,tipo,descricao,valor,ano,ativo,criado_em,criado_por,alterado_em,alterado_por,criado_rf,alterado_rf)
SELECT 'ExecucaoConsolidacaoRegistrosPedagogicos',82,'Data da �ltima execu��o da rotina de consolida��o de registros pedag�gicos','',2022,true,current_timestamp,'SISTEMA',null,'Sistema','0','0'
WHERE
    NOT EXISTS (
        SELECT ano FROM public.parametros_sistema WHERE ano = 2022
    );