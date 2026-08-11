# Сборка документации

Для сборки нужны Node.js 20 или новее, Ruby и `asciidoctor-pdf`. Используйте предварительные (alpha/testing) версии Antora и PDF Extension, а не стабильную Antora 3.1.

```bash
gem install asciidoctor-pdf
npm install -g antora@testing @antora/pdf-extension
```

Сборка трёх PDF-книг:

```bash
./docs/build-pdf.sh
```
