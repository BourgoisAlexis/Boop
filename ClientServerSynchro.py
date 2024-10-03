import shutil
import os


def replace(path: str):
    with open(path, "r") as file:
        data = file.read()
        data = data.replace("using PlayerIOClient", "using PlayerIO.GameLibrary")

    with open(path, "w") as file:
        file.write(data)


# EXECUTION
base_path: str = "Z:\PROJETS PERSOS\Boop"
client_specific_path: str = "Boop ClientSide\Assets\_Scripts\CommonCode"
server_specific_path: str = "Boop ServerSide\Serverside Code\Game Code\CommonCode"

for name in os.listdir(f"{base_path}\{client_specific_path}"):
    if not ".meta" in name:
        origin: str = f"{base_path}\{client_specific_path}\{name}"
        destination: str = f"{base_path}\{server_specific_path}\{name}"
        shutil.copy(origin, destination)
        replace(destination)
        print(name)


print("-DONE-")