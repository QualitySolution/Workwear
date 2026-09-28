# Сборка документации

Для сборки нужны Node.js 20 или новее и Ruby. Скрипт `build-pdf.sh` автоматически установит `asciidoctor-pdf`, если gem отсутствует для активной версии Ruby. Используйте предварительные (alpha/testing) версии Antora и PDF Extension, а не стабильную Antora 3.1.

```bash
npm install -g antora@testing @antora/pdf-extension
```

Сборка трёх PDF-книг:

```bash
./docs/build-pdf.sh
```
