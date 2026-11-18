"""
Gráfica de la Prueba 2: error relativo de energía a lo largo de 1000 órbitas.

Uso (desde F:\\CataclismosGravitacionales, con el .venv activado):
    python graficos/energia_largo_plazo.py

Lee  resultados/energia_largo_plazo.csv
Crea resultados/fig_energia_largo_plazo.png
"""
from pathlib import Path

import matplotlib.pyplot as plt
import pandas as pd

RAIZ = Path(__file__).resolve().parent.parent
CSV = RAIZ / "resultados" / "energia_largo_plazo.csv"
PNG = RAIZ / "resultados" / "fig_energia_largo_plazo.png"

# Orden y colores fijos por método (el color sigue al método, nunca al orden)
METODOS = {
    "Euler":       ("Euler (h)",             "#2a78d6", "-"),
    "RK4":         ("RK4 (h)",               "#eb6834", "-"),
    "Leapfrog":    ("Leapfrog (h)",          "#1baf7a", "-"),
    "Leapfrog_h4": ("Leapfrog (h/4) · mismo costo que RK4", "#eda100", "--"),
}

TEXTO = "#2b2b2a"
TEXTO_SEC = "#6b6a65"
REJILLA = "#e4e3df"
FONDO = "#fcfcfb"


def main() -> None:
    df = pd.read_csv(CSV)

    fig, ax = plt.subplots(figsize=(9, 5.2), dpi=200)
    fig.patch.set_facecolor(FONDO)
    ax.set_facecolor(FONDO)

    for clave, (etiqueta, color, estilo) in METODOS.items():
        d = df[df["metodo"] == clave]
        if d.empty:
            continue
        ax.plot(d["orbita"], d["error_energia_max"], color=color, lw=1.6,
                ls=estilo, label=etiqueta, solid_capstyle="round")
        # Etiqueta directa al final de cada curva (la identidad no depende solo del color)
        x_fin, y_fin = d["orbita"].iloc[-1], d["error_energia_max"].iloc[-1]
        ax.annotate(etiqueta.split(" ·")[0], (x_fin, y_fin), xytext=(6, 0),
                    textcoords="offset points", va="center", fontsize=8.5, color=TEXTO)

    ax.set_xscale("log")
    ax.set_yscale("log")
    ax.set_xlabel("Número de órbitas", color=TEXTO_SEC)
    ax.set_ylabel("Error relativo de energía  |E(t) − E₀| / |E₀|", color=TEXTO_SEC)
    ax.set_title("Conservación de energía en una órbita circular (1000 vueltas)",
                 color=TEXTO, loc="left", fontsize=12, pad=12)

    ax.grid(True, which="major", color=REJILLA, lw=0.8)
    ax.grid(False, which="minor")
    for lado in ("top", "right"):
        ax.spines[lado].set_visible(False)
    for lado in ("left", "bottom"):
        ax.spines[lado].set_color(REJILLA)
    ax.tick_params(colors=TEXTO_SEC, labelsize=8.5)

    ax.set_xlim(1, 1000 * 4)  # espacio a la derecha para las etiquetas
    ax.legend(frameon=False, fontsize=8.5, labelcolor=TEXTO, loc="upper center",
              bbox_to_anchor=(0.5, -0.14), ncol=4)

    fig.tight_layout()
    fig.savefig(PNG, facecolor=FONDO)
    print(f"Gráfica guardada en: {PNG}")


if __name__ == "__main__":
    main()
