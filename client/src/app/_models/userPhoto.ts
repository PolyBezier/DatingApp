import { Photo } from "./photo";

export interface UserPhoto extends Photo {
    username: string;
}