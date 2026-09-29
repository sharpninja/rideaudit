#!/usr/bin/bash
set +e
echo "DATE=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo "HOST=$(hostname)"
for spec in "28443 admission" "28444 counsel"; do
  set -- $spec
  port=$1
  role=$2
  echo "===== CURL $role $port ====="
  curl -sk --http1.1 -m 10 -D - -o /tmp/ra-$port.body -w "http_code=%{http_code}\n" "https://192.168.0.149:$port/"
  echo "BODY=$(head -c 220 /tmp/ra-$port.body 2>/dev/null)"
  echo "===== OPENSSL $role $port tls1_2 ====="
  echo | openssl s_client -connect "192.168.0.149:$port" -tls1_2 2>/tmp/ra-$port.err | openssl x509 -noout -subject -issuer -dates -ext subjectAltName
  grep -E 'Protocol|Cipher    |Verify return code' /tmp/ra-$port.err
done
echo "===== ROOT ====="
docker exec rideaudit-caddy cat /data/caddy/pki/authorities/local/root.crt > /tmp/ra-root.crt
openssl x509 -in /tmp/ra-root.crt -noout -subject -issuer
echo "===== OCTOPUS CADDY ====="
docker ps --filter name=octopus-legion2-octopus-api-tls-1 --format '{{.Names}}|{{.Status}}|{{.Ports}}'
echo "===== PLAINTEXT ====="
curl -s -m 8 -o /dev/null -w "http_28080=%{http_code}\n" http://192.168.0.149:28080/
curl -s -m 8 -o /dev/null -w "http_28081=%{http_code}\n" http://192.168.0.149:28081/
curl -s -m 8 -o /dev/null -w "http_18080=%{http_code}\n" http://127.0.0.1:18080/
echo FINAL_HOST_OK
