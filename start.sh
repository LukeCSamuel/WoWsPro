# Runs all processes required for psuedo-HMR of Blazor app
xargs -P 3 -I {} sh -c 'eval "$1"' - {} <<'EOF'

npm run sass-watch

npm run client
sleep 5; npm run server
EOF