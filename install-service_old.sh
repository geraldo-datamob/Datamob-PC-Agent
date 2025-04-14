#!/bin/bash

# ========================
# CONFIGURAÇÕES
# ========================
SERVICE_NAME="telemetry-collector"
PROJECT_NAME="TelemetryCollector.App"
VERSION="1.0.0"
RUNTIME="linux-x64"
INSTALL_DIR="/opt/$SERVICE_NAME"
TAR_FILE="TelemetryCollector.App-${VERSION}-${RUNTIME}.tar.gz"
TAR_URL="https://github.com/geraldo-datamob/Datamob-PC-Agent/releases/download/v${VERSION}/${TAR_FILE}"

# ========================
# FUNÇÃO: INSTALAR PACOTE VIA GERENCIADOR DISPONÍVEL
# ========================
install_package() {
    PACKAGE_NAME="$1"

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
}

# ========================
# VERIFICAR E INSTALAR WGET
# ========================
if ! command -v wget &> /dev/null; then
    echo "⚠️ 'wget' não está instalado. Tentando instalar automaticamente..."
    install_package wget

    if ! command -v wget &> /dev/null; then
        echo "❌ Falha ao instalar o 'wget'. Instale manualmente e tente novamente."
        exit 1
    fi
fi

# ========================
# BAIXAR ARQUIVO DO GITHUB
# ========================
echo "⬇️ Verificando se $TAR_FILE já existe..."

if [ -f "$TAR_FILE" ]; then
    echo "✅ O arquivo $TAR_FILE já existe. Pulando download."
else
    echo "⬇️ Baixando pacote do GitHub Releases..."
    wget -O "$TAR_FILE" "$TAR_URL"

    if [ ! -f "$TAR_FILE" ]; then
        echo "❌ Erro: o arquivo $TAR_FILE não foi baixado."
        exit 1
    fi
fi

# ========================
# EXTRAIR
# ========================
echo "📦 Extraindo pacote para: $INSTALL_DIR"
sudo mkdir -p "$INSTALL_DIR"
sudo tar -xzvf "$TAR_FILE" -C "$INSTALL_DIR"


# ========================
# VERIFICAR BINÁRIO
# ========================
BIN_PATH="$INSTALL_DIR/$PROJECT_NAME"
if [ ! -f "$BIN_PATH" ]; then
    echo "❌ Erro: o binário $PROJECT_NAME não foi encontrado em $INSTALL_DIR"
    exit 1
fi

if [ ! -x "$BIN_PATH" ]; then
    echo "⚙️ Tornando o binário executável..."
    sudo chmod +x "$BIN_PATH"
fi

# # ========================
# CRIAR SERVIÇO SYSTEMD
# ========================
SERVICE_FILE="/etc/systemd/system/$SERVICE_NAME.service"

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

# ========================
# ATIVAR E INICIAR O SERVIÇO
# ========================
echo "🚀 Ativando e iniciando o serviço $SERVICE_NAME..."

sudo systemctl daemon-reload
sudo systemctl enable $SERVICE_NAME
sudo systemctl restart $SERVICE_NAME

# ========================
# STATUS
# ========================
sudo systemctl status $SERVICE_NAME --no-pager
