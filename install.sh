#!/bin/bash

# Ustawienia
REPO="Xeno178531/ccommit"
INSTALL_DIR="/usr/local/bin"
BINARY_NAME="ccommit"

echo "Pobieranie najnowszej wersji $BINARY_NAME..."

# Pobranie najnowszego pliku wykonywalnego z GitHub Releases
URL=$(curl -s https://api.github.com/repos/$REPO/releases/latest | grep "browser_download_url" | grep "$BINARY_NAME\"" | cut -d '"' -f 4)

# Pobranie pliku do katalogu tymczasowego
curl -L -o /tmp/$BINARY_NAME "$URL"

# Nadanie uprawnień do uruchamiania
chmod +x /tmp/$BINARY_NAME

# Przeniesienie do katalogu systemowego (wymaga sudo)
echo "Instalowanie w $INSTALL_DIR (wymagane uprawnienia administratora)..."
sudo mv /tmp/$BINARY_NAME $INSTALL_DIR/$BINARY_NAME

echo "Gotowe! Możesz teraz wpisać: $BINARY_NAME"