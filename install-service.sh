#!/bin/bash

# ========================
# CONFIGURAÇÕES
# ========================
SERVICE_NAME="telemetry-collector"
PROJECT_NAME="TelemetryCollector.App"
INSTALL_DIR="/opt/$SERVICE_NAME"
BIN_PATH="$INSTALL_DIR/$PROJECT_NAME"
SERVICE_FILE="/etc/systemd/system/$SERVICE_NAME.service"
RUNTIME="linux-x64"

# ========================
# FUNÇÕES DE UTILIDADE
# ========================

# Função para instalar pacotes
install_package() {
    PACKAGE_NAME="$1"
    echo "⚠️ '$PACKAGE_NAME' não está instalado. Tentando instalar automaticamente..."
    
    if command -v apt &> /dev/null; then
        sudo apt update && sudo apt install -y "$PACKAGE_NAME"
    elif command -v dnf &> /dev/null; then
        sudo dnf install -y "$PACKAGE_NAME"
    elif command -v yum &> /dev/null; then
        sudo yum install -y "$PACKAGE_NAME"
    elif command -v zypper &> /dev/null; then
        sudo zypper install -y "$PACKAGE_NAME"
    elif command -v pacman &> /dev/null; then
        sudo pacman -Sy --noconfirm "$PACKAGE_NAME"
    else
        echo "❌ Gerenciador de pacotes não suportado. Instale '$PACKAGE_NAME' manualmente."
        exit 1
    fi

    if ! command -v "$PACKAGE_NAME" &> /dev/null; then
        echo "❌ Falha ao instalar '$PACKAGE_NAME'. Instale manualmente e tente novamente."
        exit 1
    fi
}

# Função para verificar ferramentas necessárias
check_required_tools() {
    if ! command -v wget &> /dev/null; then
        install_package wget
    fi
    
    if ! command -v curl &> /dev/null; then
        install_package curl
    fi
    
    if ! command -v jq &> /dev/null; then
        install_package jq
    fi
}

# Função para obter a versão mais recente do GitHub
get_latest_version() {
    local REPO="geraldo-datamob/Datamob-PC-Agent"
    echo "🔍 Verificando a versão mais recente no GitHub..."
    
    LATEST_VERSION=$(curl -s "https://api.github.com/repos/$REPO/releases/latest" | jq -r '.tag_name' | sed 's/^v//')
    
    if [ -z "$LATEST_VERSION" ] || [ "$LATEST_VERSION" == "null" ]; then
        echo "❌ Erro ao obter a versão mais recente. Usando versão padrão."
        LATEST_VERSION="1.0.0"  # Versão padrão caso não consiga obter
    else
        echo "✅ Versão mais recente encontrada: $LATEST_VERSION"
    fi
    
    TAR_FILE="TelemetryCollector.App-${LATEST_VERSION}-${RUNTIME}.tar.gz"
    TAR_URL="https://github.com/geraldo-datamob/Datamob-PC-Agent/releases/download/v${LATEST_VERSION}/${TAR_FILE}"
}

# Função para obter a versão atual instalada
get_installed_version() {
    if [ -f "$BIN_PATH" ] && [ -x "$BIN_PATH" ]; then
        # Tenta obter a versão do executável (assumindo que suporta --version)
        INSTALLED_VERSION=$($BIN_PATH --version 2>/dev/null | grep -oE "[0-9]+\.[0-9]+\.[0-9]+" | head -1)
        
        # Se não conseguir obter a versão do executável, tenta procurar em um arquivo version.txt
        if [ -z "$INSTALLED_VERSION" ] && [ -f "$INSTALL_DIR/version.txt" ]; then
            INSTALLED_VERSION=$(cat "$INSTALL_DIR/version.txt")
        fi
        
        # Se ainda não conseguir, assume que é a versão 0.0.0 (mais antiga que qualquer outra)
        if [ -z "$INSTALLED_VERSION" ]; then
            INSTALLED_VERSION="0.0.0"
        fi
        
        echo "✅ Versão instalada: $INSTALLED_VERSION"
    else
        echo "⚠️ Serviço não está instalado."
        INSTALLED_VERSION="0.0.0"
    fi
}

# Função para comparar versões (retorna 0 se v1 > v2, 1 caso contrário)
version_gt() {
    test "$(printf '%s\n' "$1" "$2" | sort -V | head -n 1)" != "$1"
}

# Função para baixar e instalar o agente
download_and_install() {
    echo "⬇️ Baixando pacote do GitHub: $TAR_URL"
    wget -O "$TAR_FILE" "$TAR_URL"
    
    if [ ! -f "$TAR_FILE" ]; then
        echo "❌ Erro: o arquivo $TAR_FILE não foi baixado."
        exit 1
    fi
    
    echo "📦 Extraindo pacote para: $INSTALL_DIR"
    sudo mkdir -p "$INSTALL_DIR"
    sudo tar -xzf "$TAR_FILE" -C "$INSTALL_DIR"
    
    # Salvar a versão em um arquivo para referência futura
    echo "$LATEST_VERSION" | sudo tee "$INSTALL_DIR/version.txt" > /dev/null
    
    # Limpar o arquivo baixado
    rm "$TAR_FILE"
    
    # Verificar binário
    if [ ! -f "$BIN_PATH" ]; then
        echo "❌ Erro: o binário $PROJECT_NAME não foi encontrado em $INSTALL_DIR"
        exit 1
    fi
    
    if [ ! -x "$BIN_PATH" ]; then
        echo "⚙️ Tornando o binário executável..."
        sudo chmod +x "$BIN_PATH"
    fi
    
    # Criar arquivo de serviço
    echo "🛠️ Criando arquivo de serviço: $SERVICE_FILE"
    sudo tee $SERVICE_FILE > /dev/null <<EOF
[Unit]
Description=Telemetry Collector Service
After=network.target

[Service]
Type=simple
ExecStart=$BIN_PATH
WorkingDirectory=$INSTALL_DIR
Restart=on-failure
RestartSec=5
User=$USER

[Install]
WantedBy=multi-user.target
EOF
    
    # Recarregar systemd, habilitar e iniciar o serviço
    echo "🚀 Ativando e iniciando o serviço $SERVICE_NAME..."
    sudo systemctl daemon-reload
    sudo systemctl enable $SERVICE_NAME
    sudo systemctl restart $SERVICE_NAME
    
    echo "✅ Instalação/atualização concluída com sucesso!"
}

# Função para parar o serviço
stop_service() {
    echo "🛑 Parando o serviço $SERVICE_NAME..."
    sudo systemctl stop $SERVICE_NAME
    
    # Aguardar um momento para garantir que o serviço parou
    sleep 2
}

# Função para verificar se o serviço está em execução
is_service_running() {
    if systemctl is-active --quiet $SERVICE_NAME; then
        return 0  # Serviço está rodando
    else
        return 1  # Serviço não está rodando
    fi
}

# Função para remover instalação existente
remove_existing_installation() {
    echo "🗑️ Removendo instalação existente..."
    if [ -d "$INSTALL_DIR" ]; then
        sudo rm -rf "$INSTALL_DIR"
    fi
}

# Função para perguntar ao usuário
ask_user() {
    local QUESTION="$1"
    local RESPONSE
    
    echo -n "$QUESTION (s/n): "
    read RESPONSE
    
    if [[ "$RESPONSE" =~ ^[Ss]$ ]]; then
        return 0  # Usuário respondeu sim
    else
        return 1  # Usuário respondeu não ou inválido
    fi
}

# ========================
# SCRIPT PRINCIPAL
# ========================

# Verificar ferramentas necessárias
check_required_tools

# Obter a versão mais recente
get_latest_version

# Obter a versão instalada
get_installed_version

# Lógica principal
if is_service_running; then
    echo "🔄 O serviço $SERVICE_NAME está em execução."
    
    # Verificar se a versão instalada é mais antiga
    if version_gt "$LATEST_VERSION" "$INSTALLED_VERSION"; then
        echo "🆙 Uma versão mais recente está disponível ($LATEST_VERSION > $INSTALLED_VERSION)."
        echo "🔄 Atualizando para a versão mais recente..."
        
        # Parar o serviço
        stop_service
        
        # Baixar e instalar a nova versão
        download_and_install
    else
        echo "✅ A versão mais recente já está instalada."
        
        # Perguntar se o usuário quer reinstalar
        if ask_user "Deseja reinstalar o serviço?"; then
            echo "🔄 Reinstalando o serviço..."
            
            # Parar o serviço
            stop_service
            
            # Remover instalação existente
            remove_existing_installation
            
            # Baixar e instalar novamente
            download_and_install
        else
            echo "✓ Nenhuma ação necessária. Serviço continua em execução."
        fi
    fi
else
    echo "⚠️ O serviço $SERVICE_NAME não está em execução."
    
    # Verificar se existem pastas do instalador
    if [ -d "$INSTALL_DIR" ]; then
        echo "🗑️ As pastas de instalação existem, mas serão removidas."
        
        # Remover instalação existente
        remove_existing_installation
    else
        echo "📁 Nenhuma instalação anterior detectada."
    fi
    
    # Baixar e instalar a nova versão
    echo "🆕 Instalando a versão mais recente ($LATEST_VERSION)..."
    download_and_install
fi

# Mostrar status do serviço
echo "📊 Status do serviço:"
sudo systemctl status $SERVICE_NAME --no-pager