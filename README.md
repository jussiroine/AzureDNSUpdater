# Azure DNS Updater
A solution for keeping my dynamic IP updated with a static DNS name using a little bit of automation from Azure Functions. This tool is useful for when I VPN back home, the dynamic IP has often changed and without a pointer (with DNS) it’s impossible to know where to connect.

I'm using a Raspberry Pi to run a simple shell script to resolve my current public IP address, and then the same script calls Azure Function that updates the new IP to Azure DNS. 

For step-by-step instructions on how this was built see my [blog here](https://jussiroine.com/2019/06/building-a-simple-and-secure-dns-updater-for-azure-dns-using-raspberry-pi-and-azure-functions/), and the [background here](https://jussiroine.com/2018/06/building-a-simple-and-secure-dns-updater-for-azure-dns-with-powershell/).
