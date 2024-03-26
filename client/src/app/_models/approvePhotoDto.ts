export class ApprovePhotoDto {
    constructor(
        public id: number,
        public username: string,
        public approve: boolean
    ) { }
}