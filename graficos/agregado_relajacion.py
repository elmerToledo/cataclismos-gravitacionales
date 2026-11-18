"""
Gráficas de la Prueba 3: la luna de partículas aislada se asienta por su propia gravedad.

Uso (desde F:\\CataclismosGravitacionales, con el .venv activado):
    python graficos/agregado_relajacion.py 500

Lee  resultados/agregado_relajacion_N<N>.csv   y   resultados/luna_relajada_N<N>.csv
Crea resultados/fig_agregado_relajacion_N<N>.png   y   resultados/fig_luna_3d_N<N>.png
"""
import sys
from pathlib import Path

import matplotlib.pyplot as plt
import numpy as np
import pandas as pd

RAIZ = Path(__file__).resolve().parent.parent
N = int(sys.argv[1]) if len(sys.argv) > 1 else 500
CSV = RAIZ / "resultados" / f"agregado_relajacion_N{N}.csv"
CSV_LUNA = RAIZ / "resultados" / f"luna_relajada_N{N}.csv"
PNG = RAIZ / "resultados" / f"fig_agregado_relajacion_N{N}.png"
PNG_3D = RAIZ / "resultados" / f"fig_luna_3d_N{N}.png"

AZUL, NARANJA, AGUA = "#2a78d6", "#eb6834", "#1baf7a"
TEXTO, TEXTO_SEC, REJILLA, FONDO = "#2b2b2a", "#6b6a65", "#e4e3df", "#fcfcfb"


def estilo(ax, titulo, ylabel):
    ax.set_facecolor(FONDO)
    ax.set_title(titulo, color=TEXTO, loc="left", fontsize=10.5, pad=8)
    ax.set_ylabel(ylabel, color=TEXTO_SEC, fontsize=9)
    ax.grid(True, color=REJILLA, lw=0.8)
    for lado in ("top", "right"):
        ax.spines[lado].set_visible(False)
    for lado in ("left", "bottom"):
        ax.spines[lado].set_color(REJILLA)
    ax.tick_params(colors=TEXTO_SEC, labelsize=8.5)


def graficas_evolucion(df):
    t = df["t"]  # tiempo dinámico de la luna = 1 en estas unidades
    fig, axs = plt.subplots(3, 1, figsize=(8.5, 9), dpi=200, sharex=True)
    fig.patch.set_facecolor(FONDO)

    # (a) Energía de movimiento: cae a medida que los choques disipan energía
    ax = axs[0]
    estilo(ax, "a) La luna se asienta: la energía de movimiento cae", "Energía (unidades del modelo)")
    ax.plot(t, df["e_cinetica"], color=AZUL, lw=1.6, label="traslación")
    ax.plot(t, df["e_giro"], color=NARANJA, lw=1.6, label="giro de las rocas")
    ax.set_yscale("log")
    ax.legend(frameon=False, fontsize=8.5, labelcolor=TEXTO, loc="upper right")

    # (b) Balance de energía: E + W_disipado debe quedarse constante
    ax = axs[1]
    estilo(ax, "b) Balance de energía:  |E(t) + W_disipado(t) − E₀| / |E₀|", "Error relativo")
    d = df[df["t"] > 0]  # en t = 0 el error es exactamente 0 (no se puede dibujar en escala log)
    ax.plot(d["t"], d["error_balance"], color=AGUA, lw=1.6)
    ax.set_yscale("log")

    # (c) Tamaño de la luna
    ax = axs[2]
    estilo(ax, "c) Tamaño de la luna", "Radio (radios del planeta)")
    ax.plot(t, df["radio_externo"], color=AZUL, lw=1.6, label="radio externo")
    ax.plot(t, df["radio_efectivo"], color=NARANJA, lw=1.6, label="radio efectivo")
    ax.legend(frameon=False, fontsize=8.5, labelcolor=TEXTO, loc="upper right")
    ax.set_xlabel("Tiempo (tiempos dinámicos de la luna)", color=TEXTO_SEC, fontsize=9)

    fig.suptitle(f"Luna de {N} partículas aislada", color=TEXTO, x=0.02, ha="left", fontsize=12.5)
    fig.tight_layout()
    fig.savefig(PNG, facecolor=FONDO)
    print(f"Gráfica guardada en: {PNG}")


def vista_3d(luna):
    x, y, z = luna["x"].to_numpy(), luna["y"].to_numpy(), luna["z"].to_numpy()
    radio = luna["radio"].iloc[0]
    dist = np.sqrt(x**2 + y**2 + z**2)

    fig = plt.figure(figsize=(6.5, 6.5), dpi=200)
    fig.patch.set_facecolor(FONDO)
    ax = fig.add_subplot(projection="3d")
    ax.set_facecolor(FONDO)

    # Tamaño del punto ≈ tamaño de la roca; color = distancia al centro (un solo tono, claro → oscuro)
    lim = dist.max() + radio
    puntos_por_unidad = 6.5 * 72 * 0.75 / (2 * lim)
    tam = (2 * radio * puntos_por_unidad) ** 2
    sc = ax.scatter(x, y, z, s=tam, c=dist, cmap="Blues_r", vmin=0, vmax=dist.max() * 1.25,
                    edgecolors="#ffffff", linewidths=0.3, depthshade=True)
    ax.set_xlim(-lim, lim); ax.set_ylim(-lim, lim); ax.set_zlim(-lim, lim)
    ax.set_box_aspect((1, 1, 1))
    ax.set_axis_off()
    ax.set_title(f"Luna relajada · {N} rocas", color=TEXTO, fontsize=12)
    fig.colorbar(sc, ax=ax, shrink=0.55, pad=0.02, label="distancia al centro")
    fig.tight_layout()
    fig.savefig(PNG_3D, facecolor=FONDO)
    print(f"Vista 3D guardada en: {PNG_3D}")


if __name__ == "__main__":
    graficas_evolucion(pd.read_csv(CSV))
    if CSV_LUNA.exists():
        vista_3d(pd.read_csv(CSV_LUNA))
