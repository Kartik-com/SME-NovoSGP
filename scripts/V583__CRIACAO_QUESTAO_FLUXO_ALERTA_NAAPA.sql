do $$
declare 
	questionarioId bigint;
	questaoId bigint;
	
begin
	select questionario_id 
		into questionarioId
	from secao_encaminhamento_naapa;

	update questao set ordem = 17 where questionario_id = questionarioId and ordem = 16;

	insert into questao(questionario_id, ordem, nome, placeholder, observacao, obrigatorio, tipo, opcionais, criado_em, criado_por, criado_rf, dimensao, tamanho, mascara)
		values(questionarioId, 16, 'Aplica��o do fluxo de alerta', '', '', true, 9, '', NOW(), 'SISTEMA', '0', 12, null, null)
		RETURNING id INTO questaoId;

	insert into opcao_resposta (questao_id, ordem, nome, criado_em, criado_por, criado_rf)
		values(questaoId, 1, 'Fluxo de viol�ncia', NOW(), 'SISTEMA', '0');
	insert into opcao_resposta (questao_id, ordem, nome, criado_em, criado_por, criado_rf)
		values(questaoId, 2, 'Notifica��o feita pelo NAAPA', NOW(), 'SISTEMA', '0');
	insert into opcao_resposta (questao_id, ordem, nome, criado_em, criado_por, criado_rf)
		values(questaoId, 3, 'Notifica��o feita pela escola', NOW(), 'SISTEMA', '0');
	insert into opcao_resposta (questao_id, ordem, nome, criado_em, criado_por, criado_rf)
		values(questaoId, 4, 'Busca ativa escolar', NOW(), 'SISTEMA', '0');
	insert into opcao_resposta (questao_id, ordem, nome, criado_em, criado_por, criado_rf)
		values(questaoId, 5, 'Busca ativa escolar - Unicef', NOW(), 'SISTEMA', '0');
	insert into opcao_resposta (questao_id, ordem, nome, criado_em, criado_por, criado_rf)
		values(questaoId, 6, 'Fluxo da Gravidez', NOW(), 'SISTEMA', '0');
	
	update questao set opcionais = '{"desabilitarDataFutura":true}' where questionario_id = questionarioId and ordem = 0;
	update secao_encaminhamento_naapa set nome = 'Informa��es' where questionario_id = questionarioId and ordem = 0;
end $$
	