#!/bin/bash
set -e

cd "$(dirname "$0")"

OutputDir=${1:-build/books}

if ! command -v antora >/dev/null 2>&1; then
	echo "Antora не установлена."
	exit 1
fi

if ! npm list -g --depth=0 @antora/pdf-extension >/dev/null 2>&1; then
	echo "Глобальный пакет @antora/pdf-extension не установлен."
	exit 1
fi

antora antora-playbook-pdf.yml

ExportDir=$(find build/pdf/workwear -type d -name _exports -print -quit)
if [ -z "$ExportDir" ]; then
	echo "Antora Assembler не создал каталог с PDF-книгами."
	exit 1
fi

mkdir -p "$OutputDir"
cp -v "$ExportDir/руководство-пользователя.pdf" "$OutputDir/user-guide.pdf"
cp -v "$ExportDir/руководство-администратора.pdf" "$OutputDir/admin-guide.pdf"
cp -v "$ExportDir/практическое-руководство.pdf" "$OutputDir/practical-guide.pdf"
