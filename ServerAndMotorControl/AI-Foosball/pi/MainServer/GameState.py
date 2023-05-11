class GameState:

    _game_data = {}

    def __init__(self):
        pass

    def update_game_data(self, new_data):
        self._game_data.update(new_data)
        del self._game_data["action"]
        

    def get_game_data(self, return_data):
        for k in return_data:
            if k in self._game_data: 
                return_data[k] = self._game_data[k]
            else:
                 return_data[k] = "not found"
        return return_data

    def get_all_data(self):
        return self._game_data