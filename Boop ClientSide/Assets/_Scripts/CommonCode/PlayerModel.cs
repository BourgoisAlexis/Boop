public class PlayerModel {
    public int[] pieces;

    public PlayerModel(int pieceNumber) {
        pieces = new int[2];
        pieces[0] = pieceNumber;
        pieces[1] = 0;
    }
}