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

if ! gem list -i asciidoctor-pdf >/dev/null 2>&1; then
	echo "Устанавливается gem asciidoctor-pdf для текущей версии Ruby..."
	gem install --no-document asciidoctor-pdf
fi

antora antora-playbook-pdf.yml

ExportDir=$(find build/pdf/workwear -type d -name _exports -print -quit)
if [ -z "$ExportDir" ]; then
	echo "Antora Assembler не создал каталог с PDF-книгами."
	exit 1
fi

mkdir -p "$OutputDir"
cp -v "$ExportDir/руководство-пользователя.pdf" "$OutputDir/"
cp -v "$ExportDir/руководство-администратора.pdf" "$OutputDir/"
cp -v "$ExportDir/практическое-руководство.pdf" "$OutputDir/"
