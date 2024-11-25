{
  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs?ref=nixpkgs-unstable";
  };

  outputs = { self, nixpkgs }:
    let
      system = "x86_64-linux";
      pkgs = import nixpkgs { inherit system; };
    in {
      devShells.${system}.default = pkgs.mkShell rec {
        name = "INCONNECT";
        packages = with pkgs; [
          docker
          go
          jq
        ];
        LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath packages;
      };
    };
}
