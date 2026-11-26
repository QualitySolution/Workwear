#!/bin/bash
set -euo pipefail

repo_dir=$(cd "$(dirname "$0")/.." && pwd)
release_dir="$repo_dir/Workwear/bin/Release"
output_dir=${1:-"$repo_dir/AstraLinuxDeb/bin"}
package=qs-workwear
version=$(sed -n 's/^\[assembly: AssemblyVersion("\([^"]*\)")\]$/\1/p' "$repo_dir/Workwear/Properties/AssemblyInfo.cs")

if [[ -z "$version" || ! -f "$release_dir/workwear.exe" || ! -f "$release_dir/libgrpc_csharp_ext.x64.so" ]]; then
    echo 'Для упаковки нужна сборка Workwear.sln в конфигурации Release для Linux и версия в AssemblyInfo.cs.' >&2
    exit 1
fi

package_dir=$(mktemp -d)
trap 'rm -rf "$package_dir"' EXIT
install -d "$package_dir/DEBIAN" "$package_dir/usr/lib/$package" "$package_dir/usr/bin"
cp -r "$release_dir/." "$package_dir/usr/lib/$package/"
install -D -m 644 "$repo_dir/AltLinuxRpm/workwear.desktop" "$package_dir/usr/share/applications/$package.desktop"
install -D -m 644 "$repo_dir/Workwear/icon/logo.png" "$package_dir/usr/share/pixmaps/$package.png"

cat > "$package_dir/usr/bin/$package" <<'EOF'
#!/bin/sh
mkdir -p "${XDG_CONFIG_HOME:-$HOME/.config}" || exit 1
cd /usr/lib/qs-workwear || exit 1
exec mono ./workwear.exe "$@"
EOF
chmod 755 "$package_dir/usr/bin/$package"

cat > "$package_dir/DEBIAN/control" <<EOF
Package: $package
Version: $version-1
Section: office
Priority: optional
Architecture: amd64
Maintainer: Quality Solution <support@qsolution.ru>
Depends: mono-devel, libmono-posix4.0-cil, libglade2.0-cil, fonts-dejavu, libstdc++6
Homepage: https://workwear.qsolution.ru/
Description: Программа учёта спецодежды QS: Спецодежда
 Ведёт учёт спецодежды, выдаваемой сотрудникам предприятия.
EOF

mkdir -p "$output_dir"
dpkg-deb --root-owner-group --build "$package_dir" "$output_dir/${package}_${version}-1_amd64.deb"
