namespace SME.SGP.Dominio.Constantes.MensagensNegocio
{
    public class MensagemNegocioEncaminhamentoAee
    {
        public const string ESTUDANTE_JA_POSSUI_ENCAMINHAMENTO_AEE_EM_ABERTO =
            "Estudante/Criança já possui encaminhamento AEE em aberto";

        public const string ENCAMINHAMENTO_SO_PODEM_SER_DEVOLVIDOS_NA_SITUACAO_ENCAMINHADO = "Encaminhamento só podem ser devolvidos na situação 'Encaminhado'";

        public const string NAO_FORAM_ENCONTRADOS_DADOS_DE_NECESSIDADE_ESPECIAL = "Não foram encontrados dados de necessidades especiais para o aluno no EOL";
        public const string ENCAMINHAMENTO_NAO_ENCONTRADO = "Encaminhamento não encontrado";
        public const string ENCAMINHAMENTO_NAO_PODE_SER_EXCLUIDO_NESSA_SITUACAO = "Encaminhamento só podem ser excluídos nas situações: 'Rascunho' ou 'Encaminhado'";
        public const string ENCAMINHAMENTO_NAO_PODE_SER_EXCLUIDO_PELO_USUARIO_LOGADO = "Encaminhamento só podem ser excluídos pelos gestores da ue ou pelo professor criador do encaminhamento";
        public const string ENCAMINHAMENTO_SO_PODEM_SER_DEVOLVIDO_PELA_GESTAO = "Encaminhamento só podem ser devolvidos por gestores da escola";
    }
}