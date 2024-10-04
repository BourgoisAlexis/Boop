import shutil
import os


def replace(path: str, from_client: bool):
    with open(path, "r") as file:
        data = file.read()
        if from_client:
            data = data.replace("using PlayerIOClient", "using PlayerIO.GameLibrary")
        else:
            data = data.replace("using PlayerIO.GameLibrary", "using PlayerIOClient")

    with open(path, "w") as file:
        file.write(data)


# EXECUTION
base_path: str = "Z:\PROJETS PERSOS\Boop"
client_specific_path: str = "Boop ClientSide\Assets\_Scripts\CommonCode"
server_specific_path: str = "Boop ServerSide\Serverside Code\Game Code\CommonCode"

print("USE server OR client TO DEFINE THE SOURCE")
v: str = input()

from_client: bool = v == "client"

for name in os.listdir(f"{base_path}\{client_specific_path}"):
    if not ".meta" in name:
        origin: str = f"{base_path}\{client_specific_path}\{name}"
        destination: str = f"{base_path}\{server_specific_path}\{name}"
        if from_client:
            shutil.copy(origin, destination)
            replace(destination, from_client)
        else:
            shutil.copy(destination, origin)
            replace(origin, from_client)
        print(name)

print("-DONE-")
input()