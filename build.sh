#!/bin/bash

# ========================
# CONFIGURAÇÕES
# ========================
PROJECT_NAME="TelemetryCollector.App"
VERSION="3.0.0"
RUNTIME="linux-x64"
OUTPUT_DIR="src/$PROJECT_NAME/publish"
TAR_NAME="${PROJECT_NAME}-${VERSION}-${RUNTIME}.tar.gz"
DEB_DIR="./deb-pkg"
DEB_FILE="${PROJECT_NAME}_${VERSION}_amd64.deb"
REPO="geraldo-datamob/Datamob-PC-Agent"

# ========================
# VERIFICA GH CLI
# ========================
if ! command -v gh &> /dev/null; then
  echo "❌ GitHub CLI (gh) não encontrado. Instale com: sudo apt install gh"
  exit 1
fi

# ========================
# GERAR .tar.gz
# ========================
echo "📦 Gerando pacote .tar.gz: $TAR_NAME"
tar -czf $TAR_NAME -C "$OUTPUT_DIR" .
echo "✅ Arquivo .tar.gz criado."

# ========================
# GERAR .deb
# ========================
echo "📦 Gerando pacote .deb..."

DEB_BIN_DIR="$DEB_DIR/usr/local/bin"
mkdir -p "$DEB_BIN_DIR"
cp "$OUTPUT_DIR/$PROJECT_NAME" "$DEB_BIN_DIR/"

mkdir -p "$DEB_DIR/DEBIAN"
cat > "$DEB_DIR/DEBIAN/control" <<EOF
Package: $PROJECT_NAME
Version: $VERSION
Section: base
Priority: optional
Architecture: amd64
Maintainer: None
Description: Aplicativo console .NET
EOF

dpkg-deb --build "$DEB_DIR" "$DEB_FILE"
echo "✅ Pacote .deb gerado: $DEB_FILE"

# ========================
# CRIAR RELEASE NO GITHUB
# ========================
echo "🚀 Criando release no GitHub: v$VERSION (marcada como latest)"

if gh release view "v$VERSION" --repo "$REPO" &> /dev/null; then
  echo "⚠️ Release v$VERSION já existe. Apagando para substituir..."
  gh release delete "v$VERSION" --repo "$REPO" --yes
fi

gh release create "v$VERSION" \
  "$TAR_NAME" "$DEB_FILE" \
  --repo "$REPO" \
  --title "Versão $VERSION" \
  --notes "Release automática do agente .NET - versão $VERSION" \
  --latest

echo "✅ Release publicada: https://github.com/$REPO/releases/tag/v$VERSION"
echo "✅ Esta release está marcada como 'latest'"