namespace Game.Interaction
{
    /// <summary>
    /// Kontrak untuk semua objek yang bisa di-interact oleh player.
    /// Implement interface ini di setiap MonoBehaviour interactable.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Teks yang ditampilkan di interaction prompt UI.</summary>
        string InteractionPrompt { get; }

        /// <summary>
        /// Dipanggil saat player menekan tombol interact.
        /// </summary>
        void OnInteract();

        /// <summary>
        /// Dipanggil saat raycast mulai mengenai objek ini (hover masuk).
        /// </summary>
        void OnHoverEnter();

        /// <summary>
        /// Dipanggil saat raycast berhenti mengenai objek ini (hover keluar).
        /// </summary>
        void OnHoverExit();

        /// <summary>Apakah objek ini saat ini bisa di-interact.</summary>
        bool CanInteract { get; }
    }
}
